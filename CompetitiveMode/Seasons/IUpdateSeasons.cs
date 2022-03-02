using System;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Seasons
{
	public interface IUpdateSeasons
	{
		IObservable<Unit> Update();
	}
}
