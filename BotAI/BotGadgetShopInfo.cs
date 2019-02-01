using System;
using HeavyMetalMachines.Combat.Gadget;

namespace HeavyMetalMachines.BotAI
{
	[Serializable]
	public class BotGadgetShopInfo
	{
		public GadgetSlot GadgetSlot;

		public string UpgradeName;

		public bool Recurring;
	}
}
