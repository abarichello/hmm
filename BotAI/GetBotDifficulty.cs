using System;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.Arena.Infra;
using HeavyMetalMachines.BotAI.Infra;
using HeavyMetalMachines.Match;
using Zenject;

namespace HeavyMetalMachines.BotAI
{
	public class GetBotDifficulty : IGetBotDifficulty
	{
		public BotAIGoal.BotDifficulty Get(TeamKind team)
		{
			IBotDifficultyGameArenaInfo currentArenaBotDifficulty = this._gameArenaConfigProvider.GameArenaConfig.GetCurrentArenaBotDifficulty();
			IBotDifficultyCalculator botDifficultyCalculator = this._botDifficultyCalculatorProvider.GetBotDifficultyCalculator(currentArenaBotDifficulty);
			return botDifficultyCalculator.Calculate(team);
		}

		[Inject]
		private IBotDifficultyCalculatorProvider _botDifficultyCalculatorProvider;

		[Inject]
		private IGameArenaConfigProvider _gameArenaConfigProvider;
	}
}
