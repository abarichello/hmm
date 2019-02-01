using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class DropBombInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(DropBomb);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Damage 0",
				"Damage Radius",
				"HP",
				"Duration",
				"Cooldown",
				"EP"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifierAmount(this.Damage, 0);
			case 1:
				return base.GetStatListSingleValue(this.ExplosionRadius);
			case 2:
				return base.GetStatListModifierAmount(this.WardModifiers, 0);
			case 3:
				return base.GetStatListModifier(this.LifeTime, this.LifeTimeUpgrade);
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
			if (index == 0)
			{
				return base.GetInfo(this.Damage, 0);
			}
			if (index != 2)
			{
				return base.GetInfo(index);
			}
			return base.GetInfo(this.WardModifiers, 0);
		}

		public float ExplosionRadius;

		public FXInfo AttachEffect;

		public FXInfo GroundEffect;

		public ModifierInfo[] WardModifiers;
	}
}
