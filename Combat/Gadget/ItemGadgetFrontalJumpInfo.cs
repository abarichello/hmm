using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class ItemGadgetFrontalJumpInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(ItemGadgetFrontalJump);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Range",
				"Cooldown",
				"EP"
			};
		}

		public override List<float> GetStats(int index)
		{
			if (index == 0)
			{
				return base.GetStatListModifier(this.Range, this.RangeUpgrade);
			}
			if (index == 1)
			{
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			}
			if (index != 2)
			{
				return base.GetStats(index);
			}
			return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
		}
	}
}
