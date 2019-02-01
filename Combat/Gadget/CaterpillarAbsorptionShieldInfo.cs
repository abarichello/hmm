using System;
using System.Collections.Generic;
using HeavyMetalMachines.VFX;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class CaterpillarAbsorptionShieldInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(CaterpillarAbsorptionShield);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"WardModifiers 0",
				"WardLifeTime",
				"ExplosionModifiers 0",
				"ExplosionModifiers 0 Duration",
				"ExplosionModifiers 1",
				"ExplosionModifiers 1 Duration",
				"ExplosionModifiers 2",
				"ExplosionModifiers 2 Duration",
				"ExplosionModifiers 3",
				"ExplosionModifiers 3 Duration",
				"ExplosionModifiers 4",
				"ExplosionModifiers 4 Duration",
				"ExplosionRange",
				"Cooldown",
				"EP"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifierAmount(this.WardModifiers, 0);
			case 1:
				return base.GetStatListSingleValue(this.WardLifeTime);
			case 2:
				return base.GetStatListModifierAmount(this.ExplosionModifiers, 0);
			case 3:
				return base.GetStatListModifierLifeTime(this.ExplosionModifiers, 0);
			case 4:
				return base.GetStatListModifierAmount(this.ExplosionModifiers, 1);
			case 5:
				return base.GetStatListModifierLifeTime(this.ExplosionModifiers, 1);
			case 6:
				return base.GetStatListModifierAmount(this.ExplosionModifiers, 2);
			case 7:
				return base.GetStatListModifierLifeTime(this.ExplosionModifiers, 2);
			case 8:
				return base.GetStatListModifierAmount(this.ExplosionModifiers, 3);
			case 9:
				return base.GetStatListModifierLifeTime(this.ExplosionModifiers, 3);
			case 10:
				return base.GetStatListModifierAmount(this.ExplosionModifiers, 4);
			case 11:
				return base.GetStatListModifierLifeTime(this.ExplosionModifiers, 4);
			case 12:
				return base.GetStatListModifier(this.ExplosionRange, this.ExplosionRangeUpgrade);
			case 13:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 14:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			if (index == 0)
			{
				return base.GetInfo(this.WardModifiers, 0);
			}
			if (index == 2)
			{
				return base.GetInfo(this.ExplosionModifiers, 0);
			}
			if (index != 6)
			{
				return base.GetInfo(index);
			}
			return base.GetInfo(this.ExplosionModifiers, 1);
		}

		public FXInfo WardEffect;

		public ModifierInfo[] WardModifiers;

		public ModifierInfo[] WardExtraModifiers;

		public float WardLifeTime;

		public ModifierFeedbackInfo WardFeedback;

		public FXInfo ExplosionEffect;

		public ModifierInfo[] ExplosionModifiers;

		public float ExplosionRange;

		public string ExplosionRangeUpgrade;
	}
}
