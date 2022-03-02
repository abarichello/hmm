using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.MatchMaking;
using HeavyMetalMachines.Matchmaking.Configuration;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.ReportSystem.Infra;
using HeavyMetalMachines.Social.Groups.Business;
using HeavyMetalMachines.Social.Groups.Models;
using Hoplon.Assertions;
using Hoplon.Time;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public class GetThenObservePlayerCompetitiveJoinability : IGetThenObservePlayerCompetitiveJoinability
	{
		public GetThenObservePlayerCompetitiveJoinability(IGetOrFetchPlayerState getOrFetchPlayerState, ILocalPlayerStorage playersStorage, ICurrentTime currentTime, IGetThenObserveCompetitiveQueueConfiguration getThenObserveCompetitiveQueueConfiguration, IGetThenObserveMatchmakingQueueState getThenObserveMatchmakingQueueState, IGroupStorage groupStorage, IIsGroupReadyToPlay isGroupReadyToPlay, CompetitiveModeLocalConfiguration localConfiguration, IRestrictionStorage restrictionStorage)
		{
			Assert.ConstructorParametersAreNotNull(new object[]
			{
				getOrFetchPlayerState,
				playersStorage,
				currentTime,
				getThenObserveCompetitiveQueueConfiguration,
				getThenObserveMatchmakingQueueState,
				groupStorage,
				isGroupReadyToPlay,
				localConfiguration,
				restrictionStorage
			});
			this._isGroupReadyToPlay = isGroupReadyToPlay;
			this._localConfiguration = localConfiguration;
			this._extraTimeToWaitForQueueEvent = TimeSpan.FromSeconds(3.0);
			this._restrictionStorage = restrictionStorage;
			this._getOrFetchPlayerState = getOrFetchPlayerState;
			this._playersStorage = playersStorage;
			this._currentTime = currentTime;
			this._getThenObserveCompetitiveQueueConfiguration = getThenObserveCompetitiveQueueConfiguration;
			this._getThenObserveMatchmakingQueueState = getThenObserveMatchmakingQueueState;
			this._groupStorage = groupStorage;
		}

		public IObservable<CompetitiveQueueJoinabilityResult> GetThenObserve()
		{
			return Observable.DistinctUntilChanged<CompetitiveQueueJoinabilityResult>(Observable.Select<IList<CompetitiveQueueJoinabilityResult>, CompetitiveQueueJoinabilityResult>(Observable.CombineLatest<CompetitiveQueueJoinabilityResult>(new IObservable<CompetitiveQueueJoinabilityResult>[]
			{
				this.IsQueueOpen(),
				this.PlayerHasModeUnlocked(),
				this.IsPlayerNotInQueue(),
				this.IsPlayerSoloOrInCompetitiveCompliedGroup(),
				this.IsGroupReady(),
				this.IsBannedFromCompetitiveQueue()
			}), new Func<IList<CompetitiveQueueJoinabilityResult>, CompetitiveQueueJoinabilityResult>(this.CheckResultsAndDecideFinalResult)));
		}

		private IObservable<CompetitiveQueueJoinabilityResult> IsGroupReady()
		{
			return Observable.Select<bool, CompetitiveQueueJoinabilityResult>(Observable.Merge<bool>(new IObservable<bool>[]
			{
				this._isGroupReadyToPlay.IsReady(),
				this._isGroupReadyToPlay.Observe()
			}), new Func<bool, CompetitiveQueueJoinabilityResult>(this.GetJoinabilityFromGroupReadyness));
		}

		private IObservable<CompetitiveQueueJoinabilityResult> IsBannedFromCompetitiveQueue()
		{
			return Observable.Select<List<IRestriction>, CompetitiveQueueJoinabilityResult>(this._restrictionStorage.GetAndObserve(), new Func<List<IRestriction>, CompetitiveQueueJoinabilityResult>(this.CheckBannedFromCompetitiveQueue));
		}

		private CompetitiveQueueJoinabilityResult GetJoinabilityFromGroupReadyness(bool canNavigate)
		{
			return new CompetitiveQueueJoinabilityResult
			{
				Reasons = new CompetitiveQueueUnjoinabilityReason[]
				{
					CompetitiveQueueUnjoinabilityReason.GroupMemberIsNotInMainMenu
				},
				CanJoin = canNavigate
			};
		}

		private IObservable<CompetitiveQueueJoinabilityResult> IsPlayerSoloOrInCompetitiveCompliedGroup()
		{
			return Observable.SelectMany<Group, CompetitiveQueueJoinabilityResult>(Observable.Concat<Group>(Observable.Defer<Group>(() => Observable.Return<Group>(this._groupStorage.Group)), new IObservable<Group>[]
			{
				this._groupStorage.OnGroupChanged
			}), (Group group) => this.GetJoinabilityFromCompetitiveGroup(group));
		}

		private IObservable<CompetitiveQueueJoinabilityResult> IsPlayerNotInQueue()
		{
			IObservable<MatchmakingQueueState> thenObserve = this._getThenObserveMatchmakingQueueState.GetThenObserve();
			if (GetThenObservePlayerCompetitiveJoinability.<>f__mg$cache0 == null)
			{
				GetThenObservePlayerCompetitiveJoinability.<>f__mg$cache0 = new Func<MatchmakingQueueState, CompetitiveQueueJoinabilityResult>(GetThenObservePlayerCompetitiveJoinability.GetJoinabilityFromMatchmakingState);
			}
			return Observable.Select<MatchmakingQueueState, CompetitiveQueueJoinabilityResult>(thenObserve, GetThenObservePlayerCompetitiveJoinability.<>f__mg$cache0);
		}

		private IObservable<CompetitiveQueueJoinabilityResult> IsQueueOpen()
		{
			return Observable.Switch<CompetitiveQueueJoinabilityResult>(Observable.Select<QueueConfiguration, IObservable<CompetitiveQueueJoinabilityResult>>(this._getThenObserveCompetitiveQueueConfiguration.GetThenObserve(), (QueueConfiguration queueConfiguration) => Observable.Merge<CompetitiveQueueJoinabilityResult>(new IObservable<CompetitiveQueueJoinabilityResult>[]
			{
				this.GetJoinabilityFromCompetitiveQueueConfiguration(queueConfiguration),
				this.KeepWaitingAndCheckingNextQueueEvents(queueConfiguration)
			})));
		}

		private IObservable<CompetitiveQueueJoinabilityResult> KeepWaitingAndCheckingNextQueueEvents(QueueConfiguration queueConfiguration)
		{
			return Observable.RepeatSafe<CompetitiveQueueJoinabilityResult>(Observable.Defer<CompetitiveQueueJoinabilityResult>(() => this.WaitAndCheckNextQueueEvent(queueConfiguration)));
		}

		private IObservable<CompetitiveQueueJoinabilityResult> PlayerHasModeUnlocked()
		{
			return Observable.Select<PlayerCompetitiveState, CompetitiveQueueJoinabilityResult>(this._getOrFetchPlayerState.GetFromPlayerId(this._playersStorage.Player.PlayerId), new Func<PlayerCompetitiveState, CompetitiveQueueJoinabilityResult>(this.GetFromPlayerState));
		}

		private IObservable<CompetitiveQueueJoinabilityResult> WaitAndCheckNextQueueEvent(QueueConfiguration queueConfig)
		{
			DateTime t = this._currentTime.NowServerUtc();
			for (int i = 0; i < queueConfig.QueuePeriods.Length; i++)
			{
				QueuePeriod nextQueuePeriod = queueConfig.QueuePeriods[i];
				if (t < nextQueuePeriod.OpenDateTimeUtc)
				{
					return this.GetTimerForNextQueueEventFromQueueConfiguration(nextQueuePeriod.OpenDateTimeUtc, true, nextQueuePeriod);
				}
				if (t < nextQueuePeriod.CloseDateTimeUtc)
				{
					return this.GetTimerForNextQueueEventFromQueueConfiguration(nextQueuePeriod.CloseDateTimeUtc, false, nextQueuePeriod);
				}
			}
			return Observable.Empty<CompetitiveQueueJoinabilityResult>();
		}

		private IObservable<CompetitiveQueueJoinabilityResult> GetTimerForNextQueueEventFromQueueConfiguration(DateTime nextEventTime, bool willOpen, QueuePeriod nextQueuePeriod)
		{
			TimeSpan timeSpan = nextEventTime - this._currentTime.NowServerUtc();
			timeSpan += this._extraTimeToWaitForQueueEvent;
			CompetitiveQueueJoinabilityResult result = CompetitiveQueueJoinabilityResult.CreateFromBooleanValue(willOpen, CompetitiveQueueUnjoinabilityReason.QueueIsNotOpen);
			result.NextQueuePeriod = new QueuePeriod?(nextQueuePeriod);
			return Observable.Select<long, CompetitiveQueueJoinabilityResult>(Observable.Timer(timeSpan), (long _) => result);
		}

		private IObservable<PlayerCompetitiveState[]> GetCompetitiveStateFromGroup(Group group)
		{
			return this._getOrFetchPlayerState.GetFromPlayersIds((from member in @group.Members
			select member.PlayerId).ToArray<long>());
		}

		private IObservable<CompetitiveQueueJoinabilityResult> GetJoinabilityFromCompetitiveGroup(Group group)
		{
			if (group == null)
			{
				return Observable.Return<CompetitiveQueueJoinabilityResult>(CompetitiveQueueJoinabilityResult.CreateAsJoinable());
			}
			if (this._localConfiguration.AllowedNumberOfGroupMembers <= 1)
			{
				return Observable.Return<CompetitiveQueueJoinabilityResult>(CompetitiveQueueJoinabilityResult.CreateAsUnjoinable(CompetitiveQueueUnjoinabilityReason.GroupMembersCountAboveLimit));
			}
			if (group.Leader == null || !(group.Leader.UniversalId == this._playersStorage.Player.UniversalId))
			{
				return Observable.Return<CompetitiveQueueJoinabilityResult>(CompetitiveQueueJoinabilityResult.CreateAsUnjoinable(CompetitiveQueueUnjoinabilityReason.PlayerIsNotLeaderOfGroup));
			}
			if (group.Members.Count > this._localConfiguration.AllowedNumberOfGroupMembers)
			{
				return Observable.Return<CompetitiveQueueJoinabilityResult>(CompetitiveQueueJoinabilityResult.CreateAsUnjoinable(CompetitiveQueueUnjoinabilityReason.GroupMembersCountAboveLimit));
			}
			IObservable<PlayerCompetitiveState[]> competitiveStateFromGroup = this.GetCompetitiveStateFromGroup(group);
			if (GetThenObservePlayerCompetitiveJoinability.<>f__mg$cache1 == null)
			{
				GetThenObservePlayerCompetitiveJoinability.<>f__mg$cache1 = new Func<PlayerCompetitiveState[], CompetitiveQueueJoinabilityResult>(GetThenObservePlayerCompetitiveJoinability.GetFromGroupMembersCompetitiveState);
			}
			return Observable.Select<PlayerCompetitiveState[], CompetitiveQueueJoinabilityResult>(competitiveStateFromGroup, GetThenObservePlayerCompetitiveJoinability.<>f__mg$cache1);
		}

		private static CompetitiveQueueJoinabilityResult GetFromGroupMembersCompetitiveState(PlayerCompetitiveState[] groupMembersState)
		{
			bool canJoin = groupMembersState.All((PlayerCompetitiveState state) => state.Status != 0);
			return CompetitiveQueueJoinabilityResult.CreateFromBooleanValue(canJoin, CompetitiveQueueUnjoinabilityReason.GroupMemberHasNotUnlockedCompetitive);
		}

		private static CompetitiveQueueJoinabilityResult GetJoinabilityFromMatchmakingState(MatchmakingQueueState matchmakingQueueState)
		{
			bool canJoin = matchmakingQueueState.Step == 0;
			return CompetitiveQueueJoinabilityResult.CreateFromBooleanValue(canJoin, CompetitiveQueueUnjoinabilityReason.PlayerIsAlreadyInQueue);
		}

		private CompetitiveQueueJoinabilityResult CheckResultsAndDecideFinalResult(IList<CompetitiveQueueJoinabilityResult> resultList)
		{
			List<CompetitiveQueueUnjoinabilityReason> list = new List<CompetitiveQueueUnjoinabilityReason>();
			DateTime? banEndTime = null;
			foreach (CompetitiveQueueJoinabilityResult competitiveQueueJoinabilityResult in resultList)
			{
				if (!competitiveQueueJoinabilityResult.CanJoin)
				{
					list.AddRange(competitiveQueueJoinabilityResult.Reasons);
				}
				if (competitiveQueueJoinabilityResult.BanEndTime != null)
				{
					banEndTime = competitiveQueueJoinabilityResult.BanEndTime;
				}
			}
			CompetitiveQueueJoinabilityResult competitiveQueueJoinabilityResult2 = new CompetitiveQueueJoinabilityResult();
			competitiveQueueJoinabilityResult2.CanJoin = !list.Any<CompetitiveQueueUnjoinabilityReason>();
			competitiveQueueJoinabilityResult2.Reasons = list.ToArray();
			competitiveQueueJoinabilityResult2.NextQueuePeriod = resultList.FirstOrDefault((CompetitiveQueueJoinabilityResult p) => p.NextQueuePeriod != null).NextQueuePeriod;
			competitiveQueueJoinabilityResult2.BanEndTime = banEndTime;
			return competitiveQueueJoinabilityResult2;
		}

		private IObservable<CompetitiveQueueJoinabilityResult> GetJoinabilityFromCompetitiveQueueConfiguration(QueueConfiguration queueConfiguration)
		{
			QueuePeriod currentOrNextPeriod = queueConfiguration.GetCurrentOrNextPeriod(this._currentTime);
			bool canJoin = this.IsCurrentTimeInsideQueuePeriod(currentOrNextPeriod);
			CompetitiveQueueJoinabilityResult competitiveQueueJoinabilityResult = CompetitiveQueueJoinabilityResult.CreateFromBooleanValue(canJoin, CompetitiveQueueUnjoinabilityReason.QueueIsNotOpen);
			competitiveQueueJoinabilityResult.NextQueuePeriod = new QueuePeriod?(currentOrNextPeriod);
			return Observable.Return<CompetitiveQueueJoinabilityResult>(competitiveQueueJoinabilityResult);
		}

		private bool IsCurrentTimeInsideQueuePeriod(QueuePeriod queuePeriod)
		{
			DateTime t = this._currentTime.NowServerUtc();
			return t >= queuePeriod.OpenDateTimeUtc && t < queuePeriod.CloseDateTimeUtc;
		}

		private CompetitiveQueueJoinabilityResult GetFromPlayerState(PlayerCompetitiveState playerCompetitiveState)
		{
			bool canJoin = playerCompetitiveState.Status != 0;
			return CompetitiveQueueJoinabilityResult.CreateFromBooleanValue(canJoin, CompetitiveQueueUnjoinabilityReason.ModeIsLocked);
		}

		private CompetitiveQueueJoinabilityResult CheckBannedFromCompetitiveQueue(List<IRestriction> restrictions)
		{
			CompetitiveQueueJoinabilityResult competitiveQueueJoinabilityResult = CompetitiveQueueJoinabilityResult.CreateAsJoinable();
			foreach (IRestriction restriction in restrictions)
			{
				if (restriction.Kind == 1)
				{
					competitiveQueueJoinabilityResult.CanJoin = false;
					competitiveQueueJoinabilityResult.BanEndTime = new DateTime?(restriction.EndTime);
					competitiveQueueJoinabilityResult.Reasons = new CompetitiveQueueUnjoinabilityReason[]
					{
						CompetitiveQueueUnjoinabilityReason.PlayerIsBannedFromQueue
					};
					return competitiveQueueJoinabilityResult;
				}
			}
			return competitiveQueueJoinabilityResult;
		}

		private readonly TimeSpan _extraTimeToWaitForQueueEvent;

		private readonly IGetOrFetchPlayerState _getOrFetchPlayerState;

		private readonly ILocalPlayerStorage _playersStorage;

		private readonly ICurrentTime _currentTime;

		private readonly IGetThenObserveCompetitiveQueueConfiguration _getThenObserveCompetitiveQueueConfiguration;

		private readonly IGetThenObserveMatchmakingQueueState _getThenObserveMatchmakingQueueState;

		private readonly IGroupStorage _groupStorage;

		private readonly IIsGroupReadyToPlay _isGroupReadyToPlay;

		private readonly CompetitiveModeLocalConfiguration _localConfiguration;

		private readonly IRestrictionStorage _restrictionStorage;

		[CompilerGenerated]
		private static Func<MatchmakingQueueState, CompetitiveQueueJoinabilityResult> <>f__mg$cache0;

		[CompilerGenerated]
		private static Func<PlayerCompetitiveState[], CompetitiveQueueJoinabilityResult> <>f__mg$cache1;
	}
}
