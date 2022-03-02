using System;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public interface ICompetitiveUnlockPresenter
	{
		IObservable<Unit> Show();
	}
}
