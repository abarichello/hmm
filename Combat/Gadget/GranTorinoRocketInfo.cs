using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class GranTorinoRocketInfo : ReflectiveProjectileInfo
	{
		public override Type GadgetType()
		{
			return typeof(GranTorinoRocket);
		}

		public FXInfo NormalReflectEffect;

		public FXInfo UpgradedEffect;

		public FXInfo UpgradedReflectEffect;

		public bool IsUltimateUpgraded;

		public string UltimateUpgrade;

		public float PickupMaxSpread;

		public float PickupMaxSpreadAngle;

		public int MaxPickupSteps;

		public int PickupStepsThreshold;

		public float PickupValueMultiplier;

		public string PickupValueMultiplierUpgrade;

		public float PickupLifetime;

		public string PickupLifetimeUpgrade;
	}
}
