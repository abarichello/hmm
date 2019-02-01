using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class FrontalRocketChargesInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(FrontalRocketCharges);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Primary Modifiers 0",
				"Primary Modifiers 0 Duration",
				"Secondary Modifiers 0",
				"Secondary Modifiers 0 Duration",
				"Charge Count",
				"Charge Time",
				"Stun Hits",
				"Stun Delay",
				"Cooldown",
				"EP",
				"Primary Modifiers 1",
				"Primary Modifiers 1 Duration",
				"Primary Modifiers 2",
				"Primary Modifiers 2 Duration",
				"Primary Modifiers 3",
				"Primary Modifiers 3 Duration"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifierAmount(this.PrimaryModifiers, 0);
			case 1:
				return base.GetStatListModifierLifeTime(this.PrimaryModifiers, 0);
			case 2:
				return base.GetStatListModifierAmount(this.SecondaryModifiers, 0);
			case 3:
				return base.GetStatListModifierLifeTime(this.SecondaryModifiers, 0);
			case 4:
				return base.GetStatListModifier((float)this.ChargeCount, this.ChargeCountUpgrade);
			case 5:
				return base.GetStatListModifier(this.ChargeTime, this.ChargeTimeUpgrade);
			case 6:
				return base.GetStatListModifier((float)this.StunHits, this.StunHitsUpgradeable);
			case 7:
				return base.GetStatListSingleValue(this.StunDelay);
			case 8:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 9:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			case 10:
				return base.GetStatListModifierAmount(this.PrimaryModifiers, 1);
			case 11:
				return base.GetStatListModifierLifeTime(this.PrimaryModifiers, 1);
			case 12:
				return base.GetStatListModifierAmount(this.PrimaryModifiers, 2);
			case 13:
				return base.GetStatListModifierLifeTime(this.PrimaryModifiers, 2);
			case 14:
				return base.GetStatListModifierAmount(this.PrimaryModifiers, 3);
			case 15:
				return base.GetStatListModifierLifeTime(this.PrimaryModifiers, 3);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			switch (index)
			{
			case 10:
				return base.GetInfo(this.PrimaryModifiers, 1);
			case 11:
				return base.GetInfo(this.PrimaryModifiers, 1);
			case 12:
				return base.GetInfo(this.PrimaryModifiers, 2);
			case 13:
				return base.GetInfo(this.PrimaryModifiers, 2);
			case 14:
				return base.GetInfo(this.PrimaryModifiers, 3);
			case 15:
				return base.GetInfo(this.PrimaryModifiers, 3);
			default:
				switch (index)
				{
				case 0:
					return base.GetInfo(this.PrimaryModifiers, 0);
				case 1:
					return base.GetInfo(this.PrimaryModifiers, 0);
				case 2:
					return base.GetInfo(this.SecondaryModifiers, 0);
				case 3:
					return base.GetInfo(this.SecondaryModifiers, 0);
				default:
					return base.GetInfo(index);
				}
				break;
			}
		}

		public FXInfo PrimaryEffect;

		public ModifierInfo[] PrimaryModifiers;

		public FrontalRocketChargesInfo.DirectionEnum Direction = FrontalRocketChargesInfo.DirectionEnum.Target;

		public float MoveSpeed;

		public string MoveSpeedUpgrade;

		public ModifierInfo[] SecondaryModifiers;

		public int StunHits;

		public string StunHitsUpgradeable;

		public float StunDelay;

		public enum DirectionEnum
		{
			Target = 1,
			Forward
		}
	}
}
