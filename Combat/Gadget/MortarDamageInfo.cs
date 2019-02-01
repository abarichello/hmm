using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class MortarDamageInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(MortarDamage);
		}

		public float ExplosionRadius;

		public bool FixedMoveSpeed;
	}
}
