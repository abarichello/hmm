using System;
using HeavyMetalMachines.MatchMaking;

namespace HeavyMetalMachines.Matchmaking.Queue
{
	public class CancelMatchmakingMatchSearch : ICancelMatchmakingMatchSearch
	{
		public CancelMatchmakingMatchSearch(IMatchmakingStateStorage matchmakingStateStorage)
		{
			this._matchmakingStateStorage = matchmakingStateStorage;
		}

		public void Cancel()
		{
			if (this._matchmakingStateStorage.CurrentState.Step != 1 && this._matchmakingStateStorage.CurrentState.Step != 2)
			{
				throw new InvalidOperationException("Cannot cancel matchmaking match search while matchmaking is not searching for a match.");
			}
			IMatchmakingStateStorage matchmakingStateStorage = this._matchmakingStateStorage;
			MatchmakingQueueState state = default(MatchmakingQueueState);
			state.Step = 0;
			matchmakingStateStorage.NotifyStateChanged(state);
			this._matchmakingStateStorage.CurrentSearch.Dispose();
			this._matchmakingStateStorage.CurrentSearch = null;
		}

		private readonly IMatchmakingStateStorage _matchmakingStateStorage;
	}
}
