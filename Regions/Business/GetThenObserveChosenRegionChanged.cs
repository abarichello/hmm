using System;
using UniRx;

namespace HeavyMetalMachines.Regions.Business
{
	public class GetThenObserveChosenRegionChanged : IGetThenObserveChosenRegionChanged
	{
		public IObservable<Region> GetThenObserve()
		{
			throw new NotImplementedException();
		}
	}
}
