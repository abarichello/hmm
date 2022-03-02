using System;
using HeavyMetalMachines.MatchMaking;
using UniRx;

namespace HeavyMetalMachines.Matchmaking.Queue
{
	public interface IMatchmakingStateStorage
	{
		MatchmakingQueueState CurrentState { get; }

		IDisposable CurrentSearch { get; set; }

		IObservable<MatchmakingQueueState> OnStateChanged { get; }

		IObservable<Exception> OnMatchmakingError { get; }

		void NotifyStateChanged(MatchmakingQueueState state);

		void NotifyMatchmakingError(Exception exception);
	}
}
