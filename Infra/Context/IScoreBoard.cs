using System;

namespace HeavyMetalMachines.Infra.Context
{
	public interface IScoreBoard
	{
		int ScoreRed { get; }

		int ScoreBlue { get; }

		int Round { get; }

		int RoundState { get; }
	}
}
