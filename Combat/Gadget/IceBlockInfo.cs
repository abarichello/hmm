using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class IceBlockInfo : BasicNewEffectOnEffectDeathCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(IceBlock);
		}

		public FXInfo OnWarmupLifeTimeEndEffect;

		public ModifierInfo[] OnWarmupLifeTimeEndDamage;
	}
}
