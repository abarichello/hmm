using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class CaterpillarDelayedLineDamageInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(CaterpillarDelayedLineDamage);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"WarmupLifeTime",
				"WarmupSetupTime",
				"ExplosionModifiers 0",
				"KnockbackDistance",
				"Cooldown",
				"EP"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListSingleValue(this.WarmupSeconds);
			case 1:
				return base.GetStatListSingleValue(this.WarmupSetupTime);
			case 2:
				return base.GetStatListModifierAmount(this.ExplosionModifiers, 0);
			case 3:
				return base.GetStatListModifier(this.KnockbackDistance, this.KnockbackDistanceUpgrade);
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
			if (index != 2)
			{
				return base.GetInfo(index);
			}
			return base.GetInfo(this.ExplosionModifiers, 0);
		}

		public float WarmupSetupTime;

		public FXInfo ExplosionEffect;

		public ModifierInfo[] ExplosionModifiers;

		public float ExplosionLength;

		public FXInfo KnockbackEffect;

		public float KnockbackDistance;

		public string KnockbackDistanceUpgrade;

		public float KnockbackFlyingTime;

		public float CursorDistance;
	}
}
