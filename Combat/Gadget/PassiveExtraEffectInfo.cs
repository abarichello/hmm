using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class PassiveExtraEffectInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(PassiveExtraEffect);
		}

		public GadgetSlot TargetGadget;
	}
}
