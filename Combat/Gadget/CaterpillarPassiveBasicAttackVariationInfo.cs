using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class CaterpillarPassiveBasicAttackVariationInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(CaterpillarPassiveBasicAttackVariation);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Modifiers 0",
				"Range",
				"Explosion Radius",
				"Cooldown",
				"EP"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifierAmount(this.Modifiers, 0);
			case 1:
				return base.GetStatListModifier(this.Range, this.RangeUpgrade);
			case 2:
				return base.GetStatListSingleValue(this.ExplosionRadiusForStats);
			case 3:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 4:
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
			return base.GetInfo(this.Modifiers, 0);
		}

		public ModifierInfo[] Modifiers;

		public float MoveSpeed;

		public float ExplosionRadiusForStats;
	}
}
