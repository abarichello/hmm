using System;
using HeavyMetalMachines.CompetitiveMode.Players;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public interface IProcessAndGetUpdatedCompetitiveStates
	{
		IObservable<PlayerCompetitiveState[]> Process(CompetitiveMatchResult competitiveMatchResult);
	}
}
