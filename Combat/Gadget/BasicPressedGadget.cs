using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BasicPressedGadget : ItemGadgetSelfEffect
	{
		private BasicPressedGadgetInfo MyInfo
		{
			get
			{
				return base.Info as BasicPressedGadgetInfo;
			}
		}
	}
}
