using System;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.CompetitiveMode.Matchmaking;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines.Matchmaking.Queue
{
	public class LegacySearchCompetitiveMatch : ISearchCompetitiveMatch
	{
		public LegacySearchCompetitiveMatch(GameModesGUI gameModesGui, ICheckConsolesQueueCondition checkConsolesQueueCondition, MatchData matchData, IClientButtonBILogger buttonBILogger)
		{
			this._gameModesGui = gameModesGui;
			this._checkConsolesQueueCondition = checkConsolesQueueCondition;
			this._matchData = matchData;
			this._buttonBiLogger = buttonBILogger;
		}

		public void Search(IMatchmakingMatchConfirmation matchConfirmation)
		{
			this._matchData.Kind = 3;
			if (this._checkConsolesQueueCondition.Check())
			{
				this._gameModesGui.OnClickedCompetitiveMatchButton(Platform.Current.GetExclusiveRankedQueueName());
				return;
			}
			this._gameModesGui.OnClickedCompetitiveMatchButton(GameModeTabs.Ranked);
		}

		private readonly GameModesGUI _gameModesGui;

		private readonly ICheckConsolesQueueCondition _checkConsolesQueueCondition;

		private readonly MatchData _matchData;

		private readonly IClientButtonBILogger _buttonBiLogger;
	}
}
