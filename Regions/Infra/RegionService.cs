using System;
using System.Collections.Generic;
using System.Linq;
using HeavyMetalMachines.Regions.Business;
using UniRx;
using UniRx.InternalUtil;

namespace HeavyMetalMachines.Regions.Infra
{
	public class RegionService : IRegionService
	{
		public RegionService(RegionController regionController)
		{
			this._regionController = regionController;
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.First<Unit>(Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				this._regionController.OnInitialized,
				Observable.DoOnCompleted<Unit>(Observable.Empty<Unit>(), new Action(this._regionController.Initialize))
			}));
		}

		public Region GetSelectedRegion()
		{
			return this.GetRegion(this._regionController.CurrentRegionServerPing.Region.RegionName);
		}

		public Region[] GetAllRegions()
		{
			return SingletonMonoBehaviour<RegionController>.Instance.RegionDictionary.Keys.Select(new Func<string, Region>(this.GetRegion)).ToArray<Region>();
		}

		public string GetBestRegion()
		{
			return this._regionController.GetBestServerAutomatically().Region.RegionName;
		}

		public void ChangeRegion(string regionName)
		{
			this._regionController.SetChosenRegionsInPlayerPrefs(new string[]
			{
				regionName
			});
			this._regionController.SaveRegionInPlayerPrefs(true);
		}

		public IObservable<Region[]> OnRegionsRefresh()
		{
			return Observable.Create<Region[]>(delegate(IObserver<Region[]> observer)
			{
				RegionController.RefreshRegionListDelegate onRegionsRefresh = delegate(Dictionary<string, RegionServerPing> regions)
				{
					observer.OnNext(regions.Keys.Select(new Func<string, Region>(this.GetRegion)).ToArray<Region>());
				};
				this._regionController.OnRefreshRegionList += onRegionsRefresh;
				return Disposable.Create(delegate()
				{
					this._regionController.OnRefreshRegionList -= onRegionsRefresh;
					observer = EmptyObserver<Region[]>.Instance;
				});
			});
		}

		public string GetRegionDraft(string region)
		{
			return this._regionController.GetRegionI18N(region);
		}

		public bool IsAutomaticRegionSelected()
		{
			return this._regionController.IsAutomaticRegionSelected();
		}

		private Region GetRegion(string regionName)
		{
			return new Region
			{
				Name = regionName,
				NameDraft = this._regionController.GetRegionI18N(regionName)
			};
		}

		private readonly RegionController _regionController;
	}
}
