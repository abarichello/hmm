using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class PassiveUpgradeInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(PassiveUpgrade);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Damage 0",
				"Damage 0 Duration",
				"Damage 1",
				"Damage 1 Duration",
				"Damage 2",
				"Damage 2 Duration",
				"Damage 2 DPS",
				"Damage 3",
				"Damage 3 Duration",
				"Damage 4",
				"Damage 4 Duration",
				"Damage 5",
				"Damage 5 Duration"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifierAmount(this.Damage, 0);
			case 1:
				return base.GetStatListModifierLifeTime(this.Damage, 0);
			case 2:
				return base.GetStatListModifierAmount(this.Damage, 1);
			case 3:
				return base.GetStatListModifierLifeTime(this.Damage, 1);
			case 4:
				return base.GetStatListModifierAmount(this.Damage, 2);
			case 5:
				return base.GetStatListModifierLifeTime(this.Damage, 2);
			case 6:
				return base.GetStatListModifierAmountPerSecond(this.Damage, 2);
			case 7:
				return base.GetStatListModifierAmount(this.Damage, 3);
			case 8:
				return base.GetStatListModifierLifeTime(this.Damage, 3);
			case 9:
				return base.GetStatListModifierAmount(this.Damage, 4);
			case 10:
				return base.GetStatListModifierLifeTime(this.Damage, 4);
			case 11:
				return base.GetStatListModifierAmount(this.Damage, 5);
			case 12:
				return base.GetStatListModifierLifeTime(this.Damage, 5);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetInfo(this.Damage, 0);
			case 1:
				return base.GetInfo(this.Damage, 0);
			case 2:
				return base.GetInfo(this.Damage, 2);
			case 3:
				return base.GetInfo(this.Damage, 2);
			case 4:
				return base.GetInfo(this.Damage, 2);
			case 5:
				return base.GetInfo(this.Damage, 4);
			case 6:
				return base.GetInfo(this.Damage, 4);
			default:
				return base.GetInfo(index);
			}
		}
	}
}
