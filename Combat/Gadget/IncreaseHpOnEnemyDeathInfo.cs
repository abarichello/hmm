using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class IncreaseHpOnEnemyDeathInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(IncreaseHpOnEnemyDeath);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Damage 0",
				"Damage 1",
				"Range"
			};
		}

		public override List<float> GetStats(int index)
		{
			if (index == 0)
			{
				return base.GetStatListModifierAmount(this.UpgradeModifiers, 0);
			}
			if (index == 1)
			{
				return base.GetStatListModifierAmount(this.UpgradeModifiers, 1);
			}
			if (index != 2)
			{
				return base.GetStats(index);
			}
			return base.GetStatListModifier(this.Range, this.RangeUpgrade);
		}

		public override ModifierInfo GetInfo(int index)
		{
			if (index == 0)
			{
				return base.GetInfo(this.UpgradeModifiers, 0);
			}
			if (index != 1)
			{
				return base.GetInfo(index);
			}
			return base.GetInfo(this.UpgradeModifiers, 1);
		}

		public ModifierInfo[] UpgradeModifiers;
	}
}
