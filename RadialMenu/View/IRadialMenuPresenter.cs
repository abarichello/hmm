using System;
using UniRx;

namespace HeavyMetalMachines.RadialMenu.View
{
	public interface IRadialMenuPresenter
	{
		IObservable<Unit> Initialize();

		IObservable<Unit> Dispose();

		void Show();

		void Hide();

		void SendSelectedItem();

		IObservable<Unit> OnConfirmed();

		IObservable<Unit> OnCanceled();
	}
}
