using System;
using HeavyMetalMachines.MatchMaking;
using UniRx;

namespace HeavyMetalMachines.Matchmaking.Queue
{
	public class GetThenObserveMatchmakingQueueState : IGetThenObserveMatchmakingQueueState
	{
		public GetThenObserveMatchmakingQueueState(IMatchmakingStateStorage matchmakingStateStorage)
		{
			this._matchmakingStateStorage = matchmakingStateStorage;
		}

		public MatchmakingQueueState Get()
		{
			return this._matchmakingStateStorage.CurrentState;
		}

		public IObservable<MatchmakingQueueState> GetThenObserve()
		{
			return Observable.Concat<MatchmakingQueueState>(Observable.Return<MatchmakingQueueState>(this._matchmakingStateStorage.CurrentState), new IObservable<MatchmakingQueueState>[]
			{
				this._matchmakingStateStorage.OnStateChanged
			});
		}

		public IObservable<Exception> ObserveErrors()
		{
			return this._matchmakingStateStorage.OnMatchmakingError;
		}

		private readonly IMatchmakingStateStorage _matchmakingStateStorage;
	}
}
