using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public interface IPickupUnspawnCountBehaviourDispatch : IDispatch
	{
		void UpdatePickupInterfaceOnClient(int pickupsCounts);
	}
}
