using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class BasicPressedGadgetInfo : ItemGadgetSelfEffectInfo
	{
		public override Type GadgetType()
		{
			return typeof(BasicPressedGadget);
		}
	}
}
