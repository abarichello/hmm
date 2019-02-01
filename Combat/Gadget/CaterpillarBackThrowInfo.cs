using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class CaterpillarBackThrowInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(CaterpillarBackThrow);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"ThrowLandingModifiers 0",
				"ThrowLandingModifiers 0 Duration",
				"ThrowLandingModifiers 1",
				"ThrowLandingModifiers 1 Duration",
				"ThrowLandingModifiers 2",
				"ThrowLandingModifiers 2 Duration",
				"ThrowLandingModifiers 3",
				"ThrowLandingModifiers 3 Duration",
				"ThrowLandingModifiers 4",
				"ThrowLandingModifiers 4 Duration",
				"ThrowLandingModifiers 5",
				"ThrowLandingModifiers 5 Duration",
				"ThrowDistance",
				"ThrowLifeTime",
				"WarmupSeconds",
				"Cooldown",
				"EP",
				"MaxUnits"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifierAmount(this.ThrowLandingModifiers, 0);
			case 1:
				return base.GetStatListModifierLifeTime(this.ThrowLandingModifiers, 0);
			case 2:
				return base.GetStatListModifierAmount(this.ThrowLandingModifiers, 1);
			case 3:
				return base.GetStatListModifierLifeTime(this.ThrowLandingModifiers, 1);
			case 4:
				return base.GetStatListModifierAmount(this.ThrowLandingModifiers, 2);
			case 5:
				return base.GetStatListModifierLifeTime(this.ThrowLandingModifiers, 2);
			case 6:
				return base.GetStatListModifierAmount(this.ThrowLandingModifiers, 3);
			case 7:
				return base.GetStatListModifierLifeTime(this.ThrowLandingModifiers, 3);
			case 8:
				return base.GetStatListModifierAmount(this.ThrowLandingModifiers, 4);
			case 9:
				return base.GetStatListModifierLifeTime(this.ThrowLandingModifiers, 4);
			case 10:
				return base.GetStatListModifierAmount(this.ThrowLandingModifiers, 5);
			case 11:
				return base.GetStatListModifierLifeTime(this.ThrowLandingModifiers, 5);
			case 12:
				return base.GetStatListSingleValue(this.ThrowDistance);
			case 13:
				return base.GetStatListSingleValue(this.ThrowLifeTime);
			case 14:
				return base.GetStatListSingleValue(this.WarmupSeconds);
			case 15:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 16:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			case 17:
				return base.GetStatListModifier(this.MaxUnits, this.MaxUnitsUpgrade);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetInfo(this.ThrowLandingModifiers, 0);
			case 1:
				return base.GetInfo(this.ThrowLandingModifiers, 0);
			case 2:
				return base.GetInfo(this.ThrowLandingModifiers, 1);
			case 3:
				return base.GetInfo(this.ThrowLandingModifiers, 1);
			default:
				return base.GetInfo(index);
			}
		}

		public float WarmupRadius = 3f;

		public float WarmupOffset = 3f;

		public FXInfo ThrowEffect;

		public ModifierInfo[] ThrowLandingModifiers;

		public float ThrowDistance = 20f;

		public float ThrowLifeTime = 0.5f;

		public CaterpillarBackThrowInfo.DirectionEnum ThrowDirection = CaterpillarBackThrowInfo.DirectionEnum.Target;

		public float MaxUnits = 1f;

		public string MaxUnitsUpgrade;

		public enum DirectionEnum
		{
			Target = 1,
			Backwards,
			Up
		}
	}
}
