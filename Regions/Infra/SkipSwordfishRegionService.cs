using System;
using HeavyMetalMachines.Regions.Business;
using UniRx;

namespace HeavyMetalMachines.Regions.Infra
{
	public class SkipSwordfishRegionService : IRegionService
	{
		public IObservable<Unit> Initialize()
		{
			return Observable.ReturnUnit();
		}

		public Region GetSelectedRegion()
		{
			return SkipSwordfishRegionService._region;
		}

		public Region[] GetAllRegions()
		{
			return this._regions;
		}

		public string GetBestRegion()
		{
			return SkipSwordfishRegionService._region.Name;
		}

		public void ChangeRegion(string regionName)
		{
		}

		public IObservable<Region[]> OnRegionsRefresh()
		{
			return Observable.Return<Region[]>(this._regions);
		}

		public string GetRegionDraft(string region)
		{
			return region;
		}

		public bool IsAutomaticRegionSelected()
		{
			return false;
		}

		private static readonly Region _region = new Region
		{
			Name = "Brasil",
			NameDraft = "Brasil"
		};

		private Region[] _regions = new Region[]
		{
			SkipSwordfishRegionService._region
		};
	}
}
