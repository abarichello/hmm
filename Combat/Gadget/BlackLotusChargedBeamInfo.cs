using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class BlackLotusChargedBeamInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(BlackLotusChargedBeam);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"ChargeTime",
				"ProjectileMinDamage",
				"ProjectileMaxDamage",
				"TrapDamage 0",
				"TrapCount",
				"Cooldown",
				"EP",
				"Range"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifier(this.ChargeTime, this.ChargeTimeUpgrade);
			case 1:
				return base.GetStatListModifier(this.ProjectileMinDamage, this.ProjectileMinDamageUpgrade);
			case 2:
				return base.GetStatListModifier(this.ProjectileMaxDamage, this.ProjectileMaxDamageUpgrade);
			case 3:
				return base.GetStatListModifierAmount(this.TrapDamage, 0);
			case 4:
				return base.GetStatListModifier((float)this.TrapCount, this.TrapCountUpgrade);
			case 5:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 6:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			case 7:
				return base.GetStatListModifier(this.ProjectileRange, this.ProjectileRangeUpgrade);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			if (index == 1)
			{
				return base.GetInfo(this.ProjectileModifiers, 0);
			}
			if (index == 2)
			{
				return base.GetInfo(this.ProjectileModifiers, 0);
			}
			if (index != 3)
			{
				return base.GetInfo(index);
			}
			return base.GetInfo(this.TrapDamage, 0);
		}

		public FXInfo ChargeEffect;

		public FXInfo[] ProjectileEffect;

		public ModifierInfo[] ProjectileModifiers;

		public float ProjectileMinDamage;

		public string ProjectileMinDamageUpgrade;

		public float ProjectileMaxDamage;

		public string ProjectileMaxDamageUpgrade;

		public float ProjectileMoveSpeed;

		public float ProjectileRange;

		public string ProjectileRangeUpgrade;

		public FXInfo DropperEffect;

		public float DropTick;

		public FXInfo AudioEffect;

		public FXInfo TrapEffect;

		public ModifierInfo[] TrapDamage;

		public float TrapLifeTime;

		public int TrapCount;

		public string TrapCountUpgrade;

		public float TrapDropDistance;

		public float TrapRange;
	}
}
