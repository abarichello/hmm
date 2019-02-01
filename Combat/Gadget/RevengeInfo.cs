using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class RevengeInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(Revenge);
		}
	}
}
