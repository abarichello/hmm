using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class SprayGadgetInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(SprayGadget);
		}
	}
}
