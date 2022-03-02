using System;

namespace HeavyMetalMachines.Regions.Business
{
	public class GetServerRegion : IGetServerRegion
	{
		public GetServerRegion(IServerRegionStorage serverRegionStorage)
		{
			this._serverRegionStorage = serverRegionStorage;
		}

		public string GetRegionName()
		{
			return this._serverRegionStorage.RegionName;
		}

		private readonly IServerRegionStorage _serverRegionStorage;
	}
}
