using System;
using HeavyMetalMachines.Regions.Business;
using UniRx;

namespace HeavyMetalMachines.Regions.Infra
{
	public interface IRegionService
	{
		IObservable<Unit> Initialize();

		Region GetSelectedRegion();

		Region[] GetAllRegions();

		string GetBestRegion();

		void ChangeRegion(string regionName);

		IObservable<Region[]> OnRegionsRefresh();

		string GetRegionDraft(string region);

		bool IsAutomaticRegionSelected();
	}
}
