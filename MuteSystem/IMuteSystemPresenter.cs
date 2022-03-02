using System;
using UniRx;

namespace HeavyMetalMachines.MuteSystem
{
	public interface IMuteSystemPresenter
	{
		IObservable<Unit> Initialize();

		IObservable<Unit> Dispose();

		void Show();

		void Hide();

		bool Visible { get; }
	}
}
