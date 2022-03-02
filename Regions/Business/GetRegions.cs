using System;
using HeavyMetalMachines.Regions.Infra;

namespace HeavyMetalMachines.Regions.Business
{
	public class GetRegions : IGetRegions
	{
		public GetRegions(IRegionService regionProvider)
		{
			this._regionProvider = regionProvider;
		}

		public Region GetSelected()
		{
			return this._regionProvider.GetSelectedRegion();
		}

		public Region[] GetAll()
		{
			return this._regionProvider.GetAllRegions();
		}

		private readonly IRegionService _regionProvider;
	}
}
