using System;

namespace HeavyMetalMachines.Regions.Business
{
	public class FakeGetCurrentRegionName : IGetCurrentRegionName
	{
		public string Get()
		{
			return "fake_region_name";
		}
	}
}
