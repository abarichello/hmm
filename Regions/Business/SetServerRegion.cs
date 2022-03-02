using System;

namespace HeavyMetalMachines.Regions.Business
{
	public class SetServerRegion : ISetServerRegion
	{
		public SetServerRegion(IServerRegionStorage serverRegionStorage)
		{
			this._serverRegionStorage = serverRegionStorage;
		}

		public void SetRegionName(string regionName)
		{
			this._serverRegionStorage.RegionName = regionName;
		}

		private readonly IServerRegionStorage _serverRegionStorage;
	}
}
