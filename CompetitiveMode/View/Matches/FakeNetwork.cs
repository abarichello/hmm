using System;
using HeavyMetalMachines.Net.Infra;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class FakeNetwork : INetwork
	{
		public bool IsServer()
		{
			return false;
		}
	}
}
