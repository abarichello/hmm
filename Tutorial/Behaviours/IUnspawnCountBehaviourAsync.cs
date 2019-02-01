using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public interface IUnspawnCountBehaviourAsync : IAsync
	{
		IFuture UpdateInterfaceOnClient(int pickupsCounts);
	}
}
