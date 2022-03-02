using System;
using UniRx;

namespace HeavyMetalMachines.Frontend.Region
{
	public interface IGetThenObserveRegionPing
	{
		IObservable<RegionServerPing> GetThenObserve();
	}
}
