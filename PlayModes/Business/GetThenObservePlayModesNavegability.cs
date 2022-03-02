using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HeavyMetalMachines.MatchMaking;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Social.Groups.Business;
using HeavyMetalMachines.Social.Groups.Models;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.PlayModes.Business
{
	public class GetThenObservePlayModesNavegability : IGetThenObservePlayModesNavegability
	{
		public IObservable<PlayModesNavegabilityResult> GetThenObserve()
		{
			return Observable.DistinctUntilChanged<PlayModesNavegabilityResult>(Observable.Select<IList<PlayModesNavegabilityResult>, PlayModesNavegabilityResult>(Observable.CombineLatest<PlayModesNavegabilityResult>(new IObservable<PlayModesNavegabilityResult>[]
			{
				this.IsPlayerNotInQueue(),
				this.IsPlayerSoloOrGroupLeader(),
				this.IsGroupReady()
			}), new Func<IList<PlayModesNavegabilityResult>, PlayModesNavegabilityResult>(this.CheckResultsAndDecideFinalResult)));
		}

		private IObservable<PlayModesNavegabilityResult> IsGroupReady()
		{
			return Observable.Select<bool, PlayModesNavegabilityResult>(this.GetThenObserveGroupReadiness(), new Func<bool, PlayModesNavegabilityResult>(this.GetNavigabilityFromGroup));
		}

		private IObservable<bool> GetThenObserveGroupReadiness()
		{
			return Observable.Concat<bool>(this._isGroupReadyToPlay.IsReady(), new IObservable<bool>[]
			{
				this._isGroupReadyToPlay.Observe()
			});
		}

		private PlayModesNavegabilityResult GetNavigabilityFromGroup(bool canNavigate)
		{
			if (canNavigate)
			{
				return PlayModesNavegabilityResult.CreateAsJoinable();
			}
			return new PlayModesNavegabilityResult
			{
				Reasons = new PlayModesNavegabilityReason[]
				{
					PlayModesNavegabilityReason.GroupIsNotReady
				},
				CanNavigate = false
			};
		}

		private IObservable<PlayModesNavegabilityResult> IsPlayerNotInQueue()
		{
			IObservable<MatchmakingQueueState> thenObserve = this._getThenObserveMatchmakingQueueState.GetThenObserve();
			if (GetThenObservePlayModesNavegability.<>f__mg$cache0 == null)
			{
				GetThenObservePlayModesNavegability.<>f__mg$cache0 = new Func<MatchmakingQueueState, PlayModesNavegabilityResult>(GetThenObservePlayModesNavegability.GetNavigabilityFromMatchmakingState);
			}
			return Observable.Select<MatchmakingQueueState, PlayModesNavegabilityResult>(thenObserve, GetThenObservePlayModesNavegability.<>f__mg$cache0);
		}

		private IObservable<PlayModesNavegabilityResult> IsPlayerSoloOrGroupLeader()
		{
			return Observable.SelectMany<Group, PlayModesNavegabilityResult>(Observable.Concat<Group>(Observable.Defer<Group>(() => Observable.Return<Group>(this._groupStorage.Group)), new IObservable<Group>[]
			{
				this._groupStorage.OnGroupChanged
			}), (Group group) => this.GetNavigabilityFromGroupLeadership(group));
		}

		private static PlayModesNavegabilityResult GetNavigabilityFromMatchmakingState(MatchmakingQueueState matchmakingQueueState)
		{
			bool canJoin = matchmakingQueueState.Step == 0;
			return PlayModesNavegabilityResult.CreateFromBooleanValue(canJoin, PlayModesNavegabilityReason.PlayerIsAlreadyInQueue);
		}

		private IObservable<PlayModesNavegabilityResult> GetNavigabilityFromGroupLeadership(Group group)
		{
			if (group == null)
			{
				return Observable.Return<PlayModesNavegabilityResult>(PlayModesNavegabilityResult.CreateAsJoinable());
			}
			if (group.Leader == null || !(group.Leader.UniversalId == this._playersStorage.Player.UniversalId))
			{
				return Observable.Return<PlayModesNavegabilityResult>(PlayModesNavegabilityResult.CreateAsUnjoinable(PlayModesNavegabilityReason.PlayerIsNotLeaderOfGroup));
			}
			return Observable.Return<PlayModesNavegabilityResult>(PlayModesNavegabilityResult.CreateAsJoinable());
		}

		private PlayModesNavegabilityResult CheckResultsAndDecideFinalResult(IList<PlayModesNavegabilityResult> resultList)
		{
			List<PlayModesNavegabilityReason> list = new List<PlayModesNavegabilityReason>();
			foreach (PlayModesNavegabilityResult playModesNavegabilityResult in resultList)
			{
				if (!playModesNavegabilityResult.CanNavigate)
				{
					list.AddRange(playModesNavegabilityResult.Reasons);
				}
			}
			return new PlayModesNavegabilityResult
			{
				CanNavigate = !list.Any<PlayModesNavegabilityReason>(),
				Reasons = list.ToArray()
			};
		}

		[Inject]
		private IGetThenObserveMatchmakingQueueState _getThenObserveMatchmakingQueueState;

		[Inject]
		private IGroupStorage _groupStorage;

		[Inject]
		private ILocalPlayerStorage _playersStorage;

		[Inject]
		private IIsGroupReadyToPlay _isGroupReadyToPlay;

		[CompilerGenerated]
		private static Func<MatchmakingQueueState, PlayModesNavegabilityResult> <>f__mg$cache0;
	}
}
