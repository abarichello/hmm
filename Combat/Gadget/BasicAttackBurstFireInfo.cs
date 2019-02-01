using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class BasicAttackBurstFireInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(BasicAttackBurstFire);
		}

		public FXInfo CallbackEffect;

		public float CallbackLifeTime;
	}
}
