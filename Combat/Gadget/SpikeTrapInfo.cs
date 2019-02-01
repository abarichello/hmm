﻿using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class SpikeTrapInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(SpikeTrap);
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
				"Damage 5 Duration",
				"Extra Modifier 0",
				"Extra Modifier 0 Duration",
				"Extra Modifier 1",
				"Extra Modifier 1 Duration",
				"Extra Modifier 2",
				"Extra Modifier 2 Duration",
				"Extra Modifier 3",
				"Extra Modifier 3 Duration",
				"Extra Modifier 4",
				"Extra Modifier 4 Duration",
				"Move Speed",
				"Drop Time",
				"Drop Distance",
				"Drop Delay",
				"Drop Number",
				"Cooldown",
				"Activation Cost",
				"Life Time",
				"Extra Life Time",
				"Drain Life",
				"Range",
				"Radius",
				"Damage 0 x25"
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
			case 13:
				return base.GetStatListModifierAmount(this.ExtraModifier, 0);
			case 14:
				return base.GetStatListModifierLifeTime(this.ExtraModifier, 0);
			case 15:
				return base.GetStatListModifierAmount(this.ExtraModifier, 1);
			case 16:
				return base.GetStatListModifierLifeTime(this.ExtraModifier, 1);
			case 17:
				return base.GetStatListModifierAmount(this.ExtraModifier, 2);
			case 18:
				return base.GetStatListModifierLifeTime(this.ExtraModifier, 2);
			case 19:
				return base.GetStatListModifierAmount(this.ExtraModifier, 3);
			case 20:
				return base.GetStatListModifierLifeTime(this.ExtraModifier, 3);
			case 21:
				return base.GetStatListModifierAmount(this.ExtraModifier, 4);
			case 22:
				return base.GetStatListModifierLifeTime(this.ExtraModifier, 4);
			case 23:
				return base.GetStatListModifier(this.MoveSpeed, this.MoveSpeedUpgrade);
			case 24:
				return base.GetStatListModifier(this.DropTime, this.DropTimeUpgrade);
			case 25:
				return base.GetStatListSingleValue(this.DropDistance);
			case 26:
				return base.GetStatListSingleValue(this.DropDelay);
			case 27:
				return base.GetStatListSingleValue((float)this.DropNumber);
			case 28:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 29:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			case 30:
				return base.GetStatListModifier(this.LifeTime, this.LifeTimeUpgrade);
			case 31:
				return base.GetStatListModifier(this.ExtraLifeTime, this.ExtraLifeTimeUpgrade);
			case 32:
				return base.GetStatListModifier(this.DrainLife, this.DrainLifeUpgrade);
			case 33:
				return base.GetStatListModifier(this.Range, this.RangeUpgrade);
			case 34:
				return base.GetStatListModifier(this.Radius, this.RadiusUpgrade);
			case 35:
				return base.GetStatListModifierAmountConvoluted(this.Damage, 0, 25f);
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
				return base.GetInfo(this.Damage, 1);
			case 3:
				return base.GetInfo(this.Damage, 1);
			default:
				return base.GetInfo(index);
			}
		}

		public float DropTime;

		public string DropTimeUpgrade;

		public float DropDistance;

		public float DropDelay = 0.002f;

		public int DropNumber = 1;

		public float SpaceBetweenTraps;

		public int MaxDropSteps;

		public int DropStepsThreshold;

		public int NumLargeEffect;

		public string LargerProjectileUpgrade;

		public float DropValueIncrease;

		public string DropValueIncreaseUpgrade;

		public bool FireExtraOnEnter;

		public string FireExtraOnEnterUpgrade;

		public FXInfo EffectLarger;

		public FXInfo ExtraEffectLarger;
	}
}
