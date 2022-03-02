using System;
using UniRx;

namespace HeavyMetalMachines.DriverHelper
{
	public interface IDriverHelper
	{
		bool Visible { get; }

		IObservable<bool> VisibilityChanged();
	}
}
