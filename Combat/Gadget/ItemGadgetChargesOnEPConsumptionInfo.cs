using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class ItemGadgetChargesOnEPConsumptionInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(ItemGadgetChargesOnEPConsumption);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"ChargeModifiers 0",
				"ChargeModifiers 1",
				"ChargeMax",
				"Cooldown",
				"EP"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifierAmount(this.ChargeModifiers, 0);
			case 1:
				return base.GetStatListModifierAmount(this.ChargeModifiers, 1);
			case 2:
				return base.GetStatListModifier((float)this.ChargeMax, this.ChargeMaxUpgrade);
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
			if (index == 0)
			{
				return base.GetInfo(this.ChargeModifiers, 0);
			}
			if (index != 1)
			{
				return base.GetInfo(index);
			}
			return base.GetInfo(this.ChargeModifiers, 1);
		}

		public FXInfo ListenerEffect;

		public FXInfo ChargeUsageEffect;

		public int ChargeMax;

		public string ChargeMaxUpgrade;

		public ModifierInfo[] ChargeModifiers;
	}
}
