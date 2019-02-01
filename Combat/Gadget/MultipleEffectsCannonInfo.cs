using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class MultipleEffectsCannonInfo : BasicNewEffectOnEffectDeathCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(MultipleEffectsCannon);
		}

		public FullEffect[] AllEffects;

		public FullEffect[] AllExtraEffects;
	}
}
