using System;
using HeavyMetalMachines.CompetitiveMode.Players;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public class ProcessAndGetUpdatedCompetitiveStates : IProcessAndGetUpdatedCompetitiveStates
	{
		public ProcessAndGetUpdatedCompetitiveStates(ICompetitiveMatchResultProcessor competitiveMatchResultProcessor)
		{
			this._competitiveMatchResultProcessor = competitiveMatchResultProcessor;
		}

		public IObservable<PlayerCompetitiveState[]> Process(CompetitiveMatchResult competitiveMatchResult)
		{
			return this._competitiveMatchResultProcessor.Process(competitiveMatchResult);
		}

		private readonly ICompetitiveMatchResultProcessor _competitiveMatchResultProcessor;
	}
}
