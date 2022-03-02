using System;
using HeavyMetalMachines.MatchMaking;
using UniRx;

namespace HeavyMetalMachines.Matchmaking.Queue
{
	public class MatchmakingStateStorage : IMatchmakingStateStorage
	{
		public IDisposable CurrentSearch { get; set; }

		public MatchmakingQueueState CurrentState
		{
			get
			{
				return this._currentState;
			}
		}

		public IObservable<MatchmakingQueueState> OnStateChanged
		{
			get
			{
				return this._stateChangedSubject;
			}
		}

		public IObservable<Exception> OnMatchmakingError
		{
			get
			{
				return this._onMatchmakingErrorSubject;
			}
		}

		public void NotifyStateChanged(MatchmakingQueueState state)
		{
			this._currentState = state;
			this._stateChangedSubject.OnNext(this._currentState);
		}

		public void NotifyMatchmakingError(Exception exception)
		{
			this._onMatchmakingErrorSubject.OnNext(exception);
		}

		private readonly Subject<MatchmakingQueueState> _stateChangedSubject = new Subject<MatchmakingQueueState>();

		private readonly Subject<Exception> _onMatchmakingErrorSubject = new Subject<Exception>();

		private MatchmakingQueueState _currentState;
	}
}
