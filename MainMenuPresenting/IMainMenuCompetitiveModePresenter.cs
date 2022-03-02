using System;
using UniRx;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public interface IMainMenuCompetitiveModePresenter : IDisposable
	{
		IObservable<Unit> Initialize();

		IObservable<Unit> Show();
	}
}
