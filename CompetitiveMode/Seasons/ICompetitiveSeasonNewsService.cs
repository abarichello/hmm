using System;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Seasons
{
	public interface ICompetitiveSeasonNewsService
	{
		IObservable<bool> TryConsume(long seasonId);
	}
}
