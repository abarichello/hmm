using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class ItemGadgetMouseProjectileInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(ItemGadgetMouseProjectile);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Modifiers 0",
				"Modifiers 0 Duration",
				"Modifiers 1",
				"Modifiers 1 Duration",
				"Range",
				"MoveSpeed",
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
				return base.GetStatListModifierLifeTime(this.Modifiers, 0);
			case 2:
				return base.GetStatListModifierAmount(this.Modifiers, 1);
			case 3:
				return base.GetStatListModifierLifeTime(this.Modifiers, 1);
			case 4:
				return base.GetStatListModifier(this.Range, this.RangeUpgrade);
			case 5:
				return base.GetStatListSingleValue(this.MoveSpeed);
			case 6:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 7:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetInfo(this.Modifiers, 0);
			case 1:
				return base.GetInfo(this.Modifiers, 0);
			case 2:
				return base.GetInfo(this.Modifiers, 1);
			case 3:
				return base.GetInfo(this.Modifiers, 1);
			default:
				return base.GetInfo(index);
			}
		}

		public ModifierInfo[] Modifiers;

		public float MoveSpeed;
	}
}
