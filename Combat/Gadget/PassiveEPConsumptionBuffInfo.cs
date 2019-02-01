using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class PassiveEPConsumptionBuffInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(PassiveEPConsumptionBuff);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Damage 0",
				"Damage 0 Duration",
				"Range",
				"Duration"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifierAmount(this.Buff, 0);
			case 1:
				return base.GetStatListModifierLifeTime(this.Buff, 0);
			case 2:
				return base.GetStatListModifier(this.Range, this.RangeUpgrade);
			case 3:
				return base.GetStatListModifier(this.LifeTime, this.LifeTimeUpgrade);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			if (index == 0)
			{
				return base.GetInfo(this.Buff, 0);
			}
			if (index != 1)
			{
				return base.GetInfo(index);
			}
			return base.GetInfo(this.Buff, 0);
		}

		public ModifierInfo[] Buff;

		public bool DontMultiplyConvoluted;
	}
}
