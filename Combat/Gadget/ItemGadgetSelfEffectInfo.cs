using System;
using System.Collections.Generic;
using HeavyMetalMachines.VFX;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class ItemGadgetSelfEffectInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(ItemGadgetSelfEffect);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Damage 0",
				"Damage 0 Duration",
				"Damage 1",
				"Damage 1 Duration",
				"Damage 2",
				"Damage 2 Duration",
				"Damage 3",
				"Damage 3 Duration",
				"Damage 4",
				"Damage 4 Duration",
				"Extra Modifier 0",
				"Extra Modifier 0 Duration",
				"Extra Modifier 1",
				"Extra Modifier 1 Duration",
				"Extra Modifier 2",
				"Extra Modifier 2 Duration",
				"Extra Modifier 3",
				"Extra Modifier 3 Duration",
				"Extra Modifier 4",
				"Extra Modifier 4 Duration",
				"Primary Damage 0",
				"Primary Damage 0 x4",
				"Primary Damage 0 Duration",
				"Extra Life Time",
				"Cooldown",
				"Activation Cost",
				"Life Time",
				"Drain Life",
				"Range",
				"Radius",
				"Warmup Seconds"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifierAmount(this.Damage, 0);
			case 1:
				return base.GetStatListModifierLifeTime(this.Damage, 0);
			case 2:
				return base.GetStatListModifierAmount(this.Damage, 1);
			case 3:
				return base.GetStatListModifierLifeTime(this.Damage, 1);
			case 4:
				return base.GetStatListModifierAmount(this.Damage, 2);
			case 5:
				return base.GetStatListModifierLifeTime(this.Damage, 2);
			case 6:
				return base.GetStatListModifierAmount(this.Damage, 3);
			case 7:
				return base.GetStatListModifierLifeTime(this.Damage, 3);
			case 8:
				return base.GetStatListModifierAmount(this.Damage, 4);
			case 9:
				return base.GetStatListModifierLifeTime(this.Damage, 4);
			case 10:
				return base.GetStatListModifierAmount(this.ExtraModifier, 0);
			case 11:
				return base.GetStatListModifierLifeTime(this.ExtraModifier, 0);
			case 12:
				return base.GetStatListModifierAmount(this.ExtraModifier, 1);
			case 13:
				return base.GetStatListModifierLifeTime(this.ExtraModifier, 1);
			case 14:
				return base.GetStatListModifierAmount(this.ExtraModifier, 2);
			case 15:
				return base.GetStatListModifierLifeTime(this.ExtraModifier, 2);
			case 16:
				return base.GetStatListModifierAmount(this.ExtraModifier, 3);
			case 17:
				return base.GetStatListModifierLifeTime(this.ExtraModifier, 3);
			case 18:
				return base.GetStatListModifierAmount(this.ExtraModifier, 4);
			case 19:
				return base.GetStatListModifierLifeTime(this.ExtraModifier, 4);
			case 20:
				return base.GetStatListModifierAmount(this.Modifiers, 0);
			case 21:
				return base.GetStatListModifierAmountConvoluted(this.Modifiers, 0, 5f);
			case 22:
				return base.GetStatListModifierLifeTime(this.Modifiers, 0);
			case 23:
				return base.GetStatListModifier(this.ExtraLifeTime, this.ExtraLifeTimeUpgrade);
			case 24:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 25:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			case 26:
				return base.GetStatListModifier(this.LifeTime, this.LifeTimeUpgrade);
			case 27:
				return base.GetStatListModifier(this.DrainLife, this.DrainLifeUpgrade);
			case 28:
				return base.GetStatListModifier(this.Range, this.RangeUpgrade);
			case 29:
				return base.GetStatListModifier(this.Radius, this.RadiusUpgrade);
			case 30:
				return base.GetStatListSingleValue(this.WarmupSeconds);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			if (index == 0)
			{
				return base.GetInfo(this.Modifiers, 0);
			}
			if (index != 1)
			{
				return base.GetInfo(index);
			}
			return base.GetInfo(this.Modifiers, 0);
		}

		public ModifierInfo[] Modifiers;

		public ModifierFeedbackInfo Feedback;

		public bool ReduceDamageOnHit;

		public string ReduceDamageOnHitUpgrade;
	}
}
