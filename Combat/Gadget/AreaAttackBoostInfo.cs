using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class AreaAttackBoostInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(AreaAttackBoost);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Damage 0",
				"Radius",
				"Duration",
				"Cooldown",
				"EP",
				"Damage 1"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifierAmount(this.Damage, 0);
			case 1:
				return base.GetStatListModifier(this.Range, this.RangeUpgrade);
			case 2:
				return base.GetStatListModifier(this.LifeTime, this.LifeTimeUpgrade);
			case 3:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 4:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			case 5:
				return base.GetStatListModifierAmount(this.Damage, 1);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			if (index == 0)
			{
				return base.GetInfo(this.Damage, 0);
			}
			if (index != 5)
			{
				return base.GetInfo(index);
			}
			return base.GetInfo(this.Damage, 1);
		}

		public FXInfo HitEffect;
	}
}
