using System;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Seasons
{
	public interface IConsumeCurrentCompetitiveSeasonNews
	{
		IObservable<bool> TryConsume();
	}
}
