using System;
using UniRx;

namespace HeavyMetalMachines.Regions.Business
{
	public class RegionControllerGetThenObserveChosenRegionChanged : IGetThenObserveChosenRegionChanged
	{
		public RegionControllerGetThenObserveChosenRegionChanged(RegionController regionController)
		{
			this._regionController = regionController;
		}

		public IObservable<Region> GetThenObserve()
		{
			return Observable.Concat<Region>(Observable.Defer<Region>(() => Observable.Return<Region>(this.ConvertToRegion(this._regionController.CurrentRegionServerPing))), new IObservable<Region>[]
			{
				Observable.Select<RegionServerPing, Region>(Observable.FromEvent<RegionServerPing>(delegate(Action<RegionServerPing> handler)
				{
					this._regionController.OnRegionServerChanged += handler;
				}, delegate(Action<RegionServerPing> handler)
				{
					this._regionController.OnRegionServerChanged -= handler;
				}), new Func<RegionServerPing, Region>(this.ConvertToRegion))
			});
		}

		private Region ConvertToRegion(RegionServerPing regionServerPing)
		{
			return new Region
			{
				Name = regionServerPing.Region.RegionName
			};
		}

		private readonly RegionController _regionController;
	}
}
