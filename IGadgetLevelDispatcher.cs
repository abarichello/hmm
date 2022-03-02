using System;
using HeavyMetalMachines.Combat.Gadget;

namespace HeavyMetalMachines
{
	public interface IGadgetLevelDispatcher
	{
		void Update(int objectId, GadgetSlot slot, string upgradeName, int level);

		void SendFullData(byte address);
	}
}
