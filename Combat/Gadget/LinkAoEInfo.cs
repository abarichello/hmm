using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class LinkAoEInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(LinkAoE);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"MinDamage",
				"MaxDamage",
				"ChargeTime",
				"Cooldown",
				"EP"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifier(this.TrapMinDamage, this.TrapMinDamageUpgrade);
			case 1:
				return base.GetStatListModifier(this.TrapMaxDamage, this.TrapMaxDamageUpgrade);
			case 2:
				return base.GetStatListModifier(this.TrapChargeTime, this.TrapChargeTimeUpgrade);
			case 3:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 4:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			default:
				return base.GetStats(index);
			}
		}

		public FXInfo DropperEffect;

		public FXInfo AudioEffect;

		public FXInfo TrapEffect;

		public ModifierInfo[] TrapDamage;

		public float DropTick;

		public float TrapDropDistance;

		public int TrapCount;

		public string TrapCountUpgrade;

		public float TrapRange;

		public float TrapMaxDamage;

		public string TrapMaxDamageUpgrade;

		public float TrapMinDamage;

		public string TrapMinDamageUpgrade;

		public float TrapChargeTime;

		public string TrapChargeTimeUpgrade;
	}
}
