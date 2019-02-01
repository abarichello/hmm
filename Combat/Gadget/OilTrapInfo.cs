using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class OilTrapInfo : SpreadingTrapInfo
	{
		public override Type GadgetType()
		{
			return typeof(OilTrap);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Drop Time",
				"Duration",
				"Collision Modifier 0",
				"Primary Radius",
				"Cooldown",
				"EP"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifier(this.DropTime, this.DropTimeUpgrade);
			case 1:
				return base.GetStatListModifier(this.LifeTime, this.LifeTimeUpgrade);
			case 2:
				return base.GetStatListModifierAmount(this.CollisionModifiers, 0);
			case 3:
				return base.GetStatListSingleValue(this.PrimaryRadius);
			case 4:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 5:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			if (index != 2)
			{
				return base.GetInfo(index);
			}
			return base.GetInfo(this.CollisionModifiers, 0);
		}

		public ModifierInfo[] CollisionModifiers;

		public FXInfo CollisionEffect;

		public float CollisionLifeTime;
	}
}
