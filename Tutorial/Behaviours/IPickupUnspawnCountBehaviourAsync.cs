using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public interface IPickupUnspawnCountBehaviourAsync : IAsync
	{
		IFuture UpdatePickupInterfaceOnClient(int pickupsCounts);
	}
}
