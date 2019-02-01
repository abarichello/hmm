using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class PassiveBonusAreaBuffInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(PassiveBonusAreaBuff);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Buff",
				"Range"
			};
		}

		public override List<float> GetStats(int index)
		{
			if (index == 0)
			{
				return base.GetStatListModifierAmount(this.Buff, 0);
			}
			if (index != 1)
			{
				return base.GetStats(index);
			}
			return base.GetStatListModifier(this.Range, this.RangeUpgrade);
		}

		public override ModifierInfo GetInfo(int index)
		{
			if (index != 0)
			{
				return base.GetInfo(index);
			}
			return base.GetInfo(this.Buff, 0);
		}

		public ModifierInfo[] Buff;
	}
}
