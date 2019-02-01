using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class FireOtherGadgetOverTimeInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(FireOtherGadgetOverTime);
		}

		public GadgetSlot gadget;

		public float duration;

		public float tick;

		public float EffectDecorationDuration;

		public FXInfo FirstEffectDecoration;

		public FXInfo EffectsDecoration;

		public FXInfo LastEffectDecoration;

		public FXInfo TokenFX;
	}
}
