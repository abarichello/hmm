using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public interface IUnspawnCountBehaviourDispatch : IDispatch
	{
		void UpdateInterfaceOnClient(int pickupsCounts);
	}
}
