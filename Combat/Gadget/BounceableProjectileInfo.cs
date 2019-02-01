using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BounceableProjectileInfo : ReflectiveProjectileInfo
	{
		public override Type GadgetType()
		{
			return typeof(BounceableProjectile);
		}

		public int AdditionalBounces;

		public string AdditionalBouncesUpgrade;

		public float BounceDistance;

		public FXInfo BounceEffect;

		public ModifierInfo[] AdditionalBounceModifiers;
	}
}
