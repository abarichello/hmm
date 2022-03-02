using System;
using Assets.ClientApiObjects.Specializations;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.BotAI.Business;
using HeavyMetalMachines.Match.Infra;
using Zenject;

namespace HeavyMetalMachines.BotAI.Infra
{
	public class BotDifficultyCalculatorProvider : IBotDifficultyCalculatorProvider
	{
		public IBotDifficultyCalculator GetBotDifficultyCalculator(IBotDifficultyGameArenaInfo botDifficultyGameArenaInfo)
		{
			BotDifficultyCalculatorKind calculatorKind = botDifficultyGameArenaInfo.CalculatorKind;
			if (calculatorKind == BotDifficultyCalculatorKind.DifficultyCalculatedByOtherTeam)
			{
				return new BotDifficultyCalculatorByOtherTeam(botDifficultyGameArenaInfo, this._matchPlayersProvider);
			}
			if (calculatorKind != BotDifficultyCalculatorKind.DifficultyCalculatedByOwnTeam)
			{
				throw new ArgumentOutOfRangeException();
			}
			return new BotDifficultyCalculatorByOwnTeam(botDifficultyGameArenaInfo, this._matchPlayersProvider);
		}

		[Inject]
		private IMatchPlayersProvider _matchPlayersProvider;
	}
}
