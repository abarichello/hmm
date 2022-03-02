using System;
using HeavyMetalMachines.CompetitiveMode.Players;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking.SkipSwordfish
{
	public class SkipSwordfishCompetitiveMatchResultProcessor : ICompetitiveMatchResultProcessor
	{
		public IObservable<PlayerCompetitiveState[]> Process(CompetitiveMatchResult competitiveMatchResult)
		{
			return Observable.Return<PlayerCompetitiveState[]>(new PlayerCompetitiveState[0]);
		}
	}
}
