using System;
using System.Linq;
using HeavyMetalMachines.MatchMaking;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public class ContinuouslyCheckAndCancelCompetitiveMatchSearch : IContinuouslyCheckAndCancelCompetitiveMatchSearch
	{
		public ContinuouslyCheckAndCancelCompetitiveMatchSearch(IGetThenObservePlayerCompetitiveJoinability competitiveJoinability, IGetThenObserveMatchmakingQueueState getThenObserveMatchmakingQueueState, ICancelMatchmakingMatchSearch cancelMatchmakingMatchSearch, ILogger<ContinuouslyCheckAndCancelCompetitiveMatchSearch> logger)
		{
			this._competitiveJoinability = competitiveJoinability;
			this._getThenObserveMatchmakingQueueState = getThenObserveMatchmakingQueueState;
			this._cancelMatchmakingMatchSearch = cancelMatchmakingMatchSearch;
			this._logger = logger;
		}

		public IObservable<Unit> CheckAndCancel()
		{
			return Observable.AsUnitObservable<bool>(Observable.Where<bool>(Observable.Select<ContinuouslyCheckAndCancelCompetitiveMatchSearch.CheckAndCancelData, bool>(Observable.CombineLatest<MatchmakingQueueState, CompetitiveQueueJoinabilityResult, ContinuouslyCheckAndCancelCompetitiveMatchSearch.CheckAndCancelData>(this._getThenObserveMatchmakingQueueState.GetThenObserve(), this._competitiveJoinability.GetThenObserve(), new Func<MatchmakingQueueState, CompetitiveQueueJoinabilityResult, ContinuouslyCheckAndCancelCompetitiveMatchSearch.CheckAndCancelData>(this.MergeValues)), new Func<ContinuouslyCheckAndCancelCompetitiveMatchSearch.CheckAndCancelData, bool>(this.CancelIfNecessary)), (bool wasCanceled) => wasCanceled));
		}

		private bool CancelIfNecessary(ContinuouslyCheckAndCancelCompetitiveMatchSearch.CheckAndCancelData data)
		{
			if (data.MatchmakingQueueState.Step == null)
			{
				return false;
			}
			if (data.MatchmakingQueueState.SearchingMatchKind != 3)
			{
				return false;
			}
			if (data.JoinabilityResult.CanJoin)
			{
				return false;
			}
			bool flag = data.JoinabilityResult.Reasons.Contains(CompetitiveQueueUnjoinabilityReason.PlayerIsBannedFromQueue);
			if (flag)
			{
				return false;
			}
			bool flag2 = data.JoinabilityResult.Reasons.Contains(CompetitiveQueueUnjoinabilityReason.PlayerIsNotLeaderOfGroup);
			if (flag2)
			{
				return false;
			}
			if (!data.JoinabilityResult.Reasons.Contains(CompetitiveQueueUnjoinabilityReason.QueueIsNotOpen))
			{
				return false;
			}
			this._logger.InfoFormat("Canceling match search. MatchmakingStep={0} Reason={1}", new object[]
			{
				data.MatchmakingQueueState.Step,
				data.JoinabilityResult.Reasons
			});
			this._cancelMatchmakingMatchSearch.Cancel();
			return true;
		}

		private ContinuouslyCheckAndCancelCompetitiveMatchSearch.CheckAndCancelData MergeValues(MatchmakingQueueState queueState, CompetitiveQueueJoinabilityResult joinabilityResult)
		{
			return new ContinuouslyCheckAndCancelCompetitiveMatchSearch.CheckAndCancelData
			{
				JoinabilityResult = joinabilityResult,
				MatchmakingQueueState = queueState
			};
		}

		private readonly IGetThenObservePlayerCompetitiveJoinability _competitiveJoinability;

		private readonly IGetThenObserveMatchmakingQueueState _getThenObserveMatchmakingQueueState;

		private readonly ICancelMatchmakingMatchSearch _cancelMatchmakingMatchSearch;

		private readonly ILogger<ContinuouslyCheckAndCancelCompetitiveMatchSearch> _logger;

		private class CheckAndCancelData
		{
			public CompetitiveQueueJoinabilityResult JoinabilityResult;

			public MatchmakingQueueState MatchmakingQueueState;
		}
	}
}
