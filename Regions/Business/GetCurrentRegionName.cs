using System;
using ClientAPI;

namespace HeavyMetalMachines.Regions.Business
{
	public class GetCurrentRegionName : IGetCurrentRegionName
	{
		public GetCurrentRegionName(SwordfishClientApi clientApi)
		{
			this._clientApi = clientApi;
		}

		public string Get()
		{
			return this._clientApi.GetCurrentRegionName();
		}

		private readonly SwordfishClientApi _clientApi;
	}
}
