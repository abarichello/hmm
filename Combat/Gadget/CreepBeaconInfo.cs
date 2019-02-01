using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class CreepBeaconInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(CreepBeacon);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Creep Buff 0",
				"Creep Buff 0 Duration",
				"Target Buff 0",
				"Target Buff 0 Duration",
				"Duration",
				"Range",
				"Cooldown",
				"EP"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifierAmount(this.CreepBuffs, 0);
			case 1:
				return base.GetStatListModifierLifeTime(this.CreepBuffs, 0);
			case 2:
				return base.GetStatListModifierAmount(this.Damage, 0);
			case 3:
				return base.GetStatListModifierLifeTime(this.Damage, 0);
			case 4:
				return base.GetStatListModifier(this.LifeTime, this.LifeTimeUpgrade);
			case 5:
				return base.GetStatListModifier(this.Range, this.RangeUpgrade);
			case 6:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 7:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetInfo(this.CreepBuffs, 0);
			case 1:
				return base.GetInfo(this.CreepBuffs, 0);
			case 2:
				return base.GetInfo(this.Damage, 0);
			case 3:
				return base.GetInfo(this.Damage, 0);
			default:
				return base.GetInfo(index);
			}
		}

		public ModifierInfo[] CreepBuffs;
	}
}
