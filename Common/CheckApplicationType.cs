using System;
using Pocketverse;

namespace HeavyMetalMachines.Common
{
	public class CheckApplicationType : ICheckApplicationType
	{
		public CheckApplicationType(Network network)
		{
			this._network = network;
		}

		public bool IsClient()
		{
			return this._network.IsClient();
		}

		public bool IsServer()
		{
			return this._network.IsServer();
		}

		private readonly Network _network;
	}
}
