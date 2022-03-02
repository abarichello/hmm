using System;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode
{
	public interface IInitializeCompetitiveMode
	{
		IObservable<Unit> Initialize();
	}
}
