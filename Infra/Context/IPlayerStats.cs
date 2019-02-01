using System;
using System.Collections.Generic;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines.Infra.Context
{
	public interface IPlayerStats
	{
		int Kills { get; }

		int Deaths { get; }

		int Assists { get; }

		List<int> MissionsCompletedIndex { get; }

		bool MatchWon { get; }

		float DamageDealtToPlayers { get; }

		float HealingProvided { get; }

		int NumberOfMedals { get; }

		float GetDamagePerMinuteDealt(IMatchStats matchStats);

		float GetHealingPerMinuteProvided(IMatchStats matchStats);

		TeamKind Team { get; }
	}
}
