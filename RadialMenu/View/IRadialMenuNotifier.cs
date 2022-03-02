using System;
using UniRx;

namespace HeavyMetalMachines.RadialMenu.View
{
	public interface IRadialMenuNotifier
	{
		void Enable();

		void Disable();

		IObservable<RadialSliceChange> CurrentSliceChanged { get; }
	}
}
