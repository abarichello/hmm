using System;
using UniRx;

namespace HeavyMetalMachines.PlayModes.Business
{
	public interface IGetThenObservePlayModesNavegability
	{
		IObservable<PlayModesNavegabilityResult> GetThenObserve();
	}
}
