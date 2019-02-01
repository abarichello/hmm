using System;
using System.Collections.Generic;
using HeavyMetalMachines.VFX;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class HPRegenBoostInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(HPRegenBoost);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Charge Value",
				"Charge Duration",
				"Charge Max"
			};
		}

		public override List<float> GetStats(int index)
		{
			if (index == 0)
			{
				return base.GetStatListModifier(this.ChargeRegen, this.ChargeRegenUpgrade);
			}
			if (index == 1)
			{
				return base.GetStatListModifier(this.ChargeDuration, this.ChargeDurationUpgrade);
			}
			if (index != 2)
			{
				return base.GetStats(index);
			}
			return base.GetStatListModifier((float)this.ChargeMaxNumber, this.ChargeMaxNumberUpgrade);
		}

		public ModifierFeedbackInfo Feedback2D;

		public int ChargeMaxNumber;

		public string ChargeMaxNumberUpgrade;

		public float ChargeDuration;

		public string ChargeDurationUpgrade;

		public float ChargeRegen;

		public string ChargeRegenUpgrade;

		public float ChargeMinInterval;

		public string ChargeMinIntervalUpgrade;
	}
}
