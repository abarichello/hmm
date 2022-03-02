using System;
using UniRx;

namespace HeavyMetalMachines.Pause
{
	public interface IPausePresenter
	{
		bool Visible { get; }

		IObservable<bool> VisibilityChanged();
	}
}
