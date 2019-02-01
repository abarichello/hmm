using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class LadyMuerteHomingProjectileInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(LadyMuerteHomingProjectile);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"ProjectileModifiers 0",
				"ProjectileMoveSpeed",
				"ProjectileRange",
				"UseProjectileWithImprovedHoming",
				"Cooldown",
				"EP"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifierAmount(this.ProjectileModifiers, 0);
			case 1:
				return base.GetStatListModifier(this.ProjectileMoveSpeed, this.ProjectileMoveSpeedUpgrade);
			case 2:
				return base.GetStatListSingleValue(this.ProjectileRange);
			case 3:
				return base.GetStatListModifier(this.UseProjectileWithImprovedHoming, this.UseProjectileWithImprovedHomingUpgrade);
			case 4:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 5:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			if (index != 0)
			{
				return base.GetInfo(index);
			}
			return base.GetInfo(this.ProjectileModifiers, 0);
		}

		public ModifierInfo[] ProjectileModifiers;

		public FXInfo ProjectileEffect;

		public FXInfo ProjectileWithImprovedHomingEffect;

		public float ProjectileMoveSpeed;

		public string ProjectileMoveSpeedUpgrade;

		public float ProjectileRange;

		public float UseProjectileWithImprovedHoming;

		public string UseProjectileWithImprovedHomingUpgrade;
	}
}
