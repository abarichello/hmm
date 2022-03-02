using System;
using HeavyMetalMachines.CompetitiveMode.Players;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public interface ICompetitiveMatchResultProcessor
	{
		IObservable<PlayerCompetitiveState[]> Process(CompetitiveMatchResult competitiveMatchResult);
	}
}
