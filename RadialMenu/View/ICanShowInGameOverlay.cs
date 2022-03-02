using System;
using UniRx;

namespace HeavyMetalMachines.RadialMenu.View
{
	public interface ICanShowInGameOverlay
	{
		IObservable<bool> GetThenObserveCanShow();
	}
}
