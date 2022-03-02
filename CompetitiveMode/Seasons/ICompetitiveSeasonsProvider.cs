using System;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Seasons
{
	public interface ICompetitiveSeasonsProvider
	{
		IObservable<CompetitiveSeason> GetCurrentSeason();

		IObservable<CompetitiveSeason> GetNextSeason();
	}
}
