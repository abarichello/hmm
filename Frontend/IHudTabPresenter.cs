using System;
using UniRx;

namespace HeavyMetalMachines.Frontend
{
	public interface IHudTabPresenter
	{
		bool Visible { get; }

		IObservable<bool> VisibilityChanged();
	}
}
