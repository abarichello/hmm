using System;
using HeavyMetalMachines.Matchmaking.Queue;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public class SearchCompetitiveMatch : ISearchCompetitiveMatch
	{
		public SearchCompetitiveMatch(IMatchmakingStateStorage matchmakingStateStorage, IMatchmakingMatchSearch matchmakingMatchSearch)
		{
			this._matchmakingStateStorage = matchmakingStateStorage;
			this._matchmakingMatchSearch = matchmakingMatchSearch;
		}

		public void Search(IMatchmakingMatchConfirmation matchConfirmation)
		{
			this.AssertMatchmakingNotBusy();
			this._matchmakingMatchSearch.Search(matchConfirmation);
		}

		private void AssertMatchmakingNotBusy()
		{
			if (this._matchmakingStateStorage.CurrentState.Step != null)
			{
				throw new InvalidOperationException("Cannot start searching for a match while matchmaking is already searching for a match.");
			}
		}

		private readonly IMatchmakingStateStorage _matchmakingStateStorage;

		private readonly IMatchmakingMatchSearch _matchmakingMatchSearch;
	}
}
