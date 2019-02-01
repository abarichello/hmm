using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class PassiveExtraDamageInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(PassiveExtraDamage);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"ExtraDamage 0",
				"ExtraDamage 0 Duration",
				"ExtraDamage 1",
				"ExtraDamage 1 Duration"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifierAmount(this.ExtraDamage, 0);
			case 1:
				return base.GetStatListModifierLifeTime(this.ExtraDamage, 0);
			case 2:
				return base.GetStatListModifierAmount(this.ExtraDamage, 1);
			case 3:
				return base.GetStatListModifierLifeTime(this.ExtraDamage, 1);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetInfo(this.ExtraDamage, 0);
			case 1:
				return base.GetInfo(this.ExtraDamage, 0);
			case 2:
				return base.GetInfo(this.ExtraDamage, 1);
			case 3:
				return base.GetInfo(this.ExtraDamage, 1);
			default:
				return base.GetInfo(index);
			}
		}

		public GadgetSlot TargetGadget;

		public ModifierInfo[] ExtraDamage;
	}
}
