using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines.Matchmaking.Queue
{
	public class SearchCasualMatch : ISearchCasualMatch
	{
		public SearchCasualMatch(IMatchmakingStateStorage matchmakingStateStorage, GameModesGUI gameModesGui, ICheckNoviceQueueCondition checkNoviceQueue, MatchData matchData, ICheckConsolesQueueCondition checkConsolesQueueCondition)
		{
			this._matchmakingStateStorage = matchmakingStateStorage;
			this._gameModesGui = gameModesGui;
			this._checkNoviceQueue = checkNoviceQueue;
			this._matchData = matchData;
			this._checkConsolesQueueCondition = checkConsolesQueueCondition;
		}

		public void Search()
		{
			this.AssertMatchmakingNotBusy();
			bool flag = this._checkNoviceQueue.ShouldGoToNoviceQueue();
			if (flag)
			{
				this._matchData.Kind = 7;
				this._gameModesGui.OnClickedNormalGame(GameModeTabs.Novice);
				return;
			}
			this._matchData.Kind = 0;
			if (this._checkConsolesQueueCondition.Check())
			{
				this._gameModesGui.OnClickedNormalGame(Platform.Current.GetExclusiveCasualQueueName());
				return;
			}
			this._gameModesGui.OnClickedNormalGame(GameModeTabs.Normal);
		}

		private void AssertMatchmakingNotBusy()
		{
			if (this._matchmakingStateStorage.CurrentState.Step != null)
			{
				throw new InvalidOperationException("Cannot start searching for a match while matchmaking is already searching for a match.");
			}
		}

		private readonly IMatchmakingStateStorage _matchmakingStateStorage;

		private readonly GameModesGUI _gameModesGui;

		private readonly ICheckNoviceQueueCondition _checkNoviceQueue;

		private readonly MatchData _matchData;

		private readonly ICheckConsolesQueueCondition _checkConsolesQueueCondition;
	}
}
