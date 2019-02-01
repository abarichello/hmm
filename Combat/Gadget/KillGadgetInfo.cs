using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class KillGadgetInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(KillGadget);
		}
	}
}
