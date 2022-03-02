using System;
using UniRx;

namespace HeavyMetalMachines.Regions.Business
{
	public interface IGetThenObserveChosenRegionChanged
	{
		IObservable<Region> GetThenObserve();
	}
}
