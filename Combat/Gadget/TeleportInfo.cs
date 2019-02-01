using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class TeleportInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(Teleport);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Range",
				"Damage 0",
				"Damage 0 Duration",
				"Slow Radius",
				"Cooldown",
				"EP",
				"Damage 1",
				"Damage 1 Duration",
				"Damage 2",
				"Damage 2 Duration"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifier(this.TravelDistance, this.TravelDistanceUpgrade);
			case 1:
				return base.GetStatListModifierAmount(this.Damage, 0);
			case 2:
				return base.GetStatListModifierLifeTime(this.Damage, 0);
			case 3:
				return base.GetStatListModifier(this.Range, this.RangeUpgrade);
			case 4:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 5:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			case 6:
				return base.GetStatListModifierAmount(this.Damage, 1);
			case 7:
				return base.GetStatListModifierLifeTime(this.Damage, 1);
			case 8:
				return base.GetStatListModifierAmount(this.Damage, 2);
			case 9:
				return base.GetStatListModifierLifeTime(this.Damage, 2);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			switch (index)
			{
			case 1:
				return base.GetInfo(this.Damage, 0);
			case 2:
				return base.GetInfo(this.Damage, 0);
			case 6:
				return base.GetInfo(this.Damage, 1);
			case 7:
				return base.GetInfo(this.Damage, 1);
			}
			return base.GetInfo(index);
		}

		public float TravelDistance;

		public string TravelDistanceUpgrade;
	}
}
