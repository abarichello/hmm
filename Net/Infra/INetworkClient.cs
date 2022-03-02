using System;

namespace HeavyMetalMachines.Net.Infra
{
	public interface INetworkClient
	{
		event Action OnClientDisconnect;
	}
}
