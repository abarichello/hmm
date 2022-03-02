using System;
using HeavyMetalMachines.Net.Infra;

namespace HeavyMetalMachines.Networking
{
	public class FakeNetwork : INetwork
	{
		public FakeNetwork(bool isServer)
		{
			this.ResultOfIsServer = isServer;
		}

		public bool ResultOfIsServer { get; set; }

		public bool IsServer()
		{
			return this.ResultOfIsServer;
		}
	}
}
