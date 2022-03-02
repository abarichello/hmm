using System;
using UniRx;

namespace HeavyMetalMachines.Frontend.Region
{
	public class LegacyGetThenObserveRegionPing : IGetThenObserveRegionPing
	{
		public LegacyGetThenObserveRegionPing(RegionController regionController)
		{
			this._regionController = regionController;
		}

		public IObservable<RegionServerPing> GetThenObserve()
		{
			return Observable.Concat<RegionServerPing>(Observable.Defer<RegionServerPing>(() => Observable.Return<RegionServerPing>(this._regionController.CurrentRegionServerPing)), new IObservable<RegionServerPing>[]
			{
				this.ObserveOnRegionServerChangedEvent()
			});
		}

		private IObservable<RegionServerPing> ObserveOnRegionServerChangedEvent()
		{
			return Observable.FromEvent<RegionServerPing>(delegate(Action<RegionServerPing> handler)
			{
				this._regionController.OnRegionServerChanged += handler;
			}, delegate(Action<RegionServerPing> handler)
			{
				this._regionController.OnRegionServerChanged -= handler;
			});
		}

		private readonly RegionController _regionController;
	}
}
