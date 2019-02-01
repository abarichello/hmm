using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class PassiveEPConsumptionDualBuffInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(PassiveEPConsumptionDualBuff);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Primary Buff 0",
				"Secondary Buff 0",
				"Range",
				"Duration"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifierAmount(this.PrimaryBuff, 0);
			case 1:
				return base.GetStatListModifierAmount(this.SecondaryBuff, 0);
			case 2:
				return base.GetStatListModifier(this.Range, this.RangeUpgrade);
			case 3:
				return base.GetStatListSingleValue(this.LifeTime);
			case 4:
				return base.GetStatListModifierAmount(this.PrimaryBuff, 1);
			case 5:
				return base.GetStatListModifierAmount(this.SecondaryBuff, 1);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetInfo(this.PrimaryBuff, 0);
			case 1:
				return base.GetInfo(this.SecondaryBuff, 0);
			case 4:
				return base.GetInfo(this.PrimaryBuff, 1);
			case 5:
				return base.GetInfo(this.SecondaryBuff, 1);
			}
			return base.GetInfo(index);
		}

		public FXInfo PrimaryEffect;

		public ModifierInfo[] PrimaryBuff;

		public FXInfo SecondaryEffect;

		public ModifierInfo[] SecondaryBuff;

		public bool DontMultiplyConvoluted;
	}
}
