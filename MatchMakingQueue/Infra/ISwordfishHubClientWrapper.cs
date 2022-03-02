using System;
using ClientAPI.MessageHub;

namespace HeavyMetalMachines.MatchMakingQueue.Infra
{
	public interface ISwordfishHubClientWrapper
	{
		event Action<ConnectionInstabilityMessage> OnConnectionInstability;
	}
}
