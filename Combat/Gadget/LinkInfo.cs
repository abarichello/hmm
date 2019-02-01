using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class LinkInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(Link);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Ongoing Damage",
				"Ongoing Range",
				"Link Damage",
				"Link Damage Duration",
				"Snap Range",
				"Snap Damage",
				"Duration",
				"Cooldown",
				"EP",
				"Ongoing Damage DPS",
				"Snap Damage Duration"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifierAmount(this.OngoingDamage, 0);
			case 1:
				return base.GetStatListModifier(this.OngoingRange, this.OngoingRangeUpgrade);
			case 2:
				return base.GetStatListModifierAmount(this.LinkDamage, 0);
			case 3:
				return base.GetStatListModifierLifeTime(this.LinkDamage, 0);
			case 4:
				return base.GetStatListSingleValue(this.SnapDistance);
			case 5:
				return base.GetStatListModifierAmount(this.SnapDamage, 0);
			case 6:
				return base.GetStatListModifier(this.LifeTime, this.LifeTimeUpgrade);
			case 7:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 8:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			case 9:
				return base.GetStatListModifierAmountPerSecond(this.OngoingDamage, 0);
			case 10:
				return base.GetStatListModifierLifeTime(this.SnapDamage, 0);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetInfo(this.OngoingDamage, 0);
			case 2:
				return base.GetInfo(this.LinkDamage, 0);
			case 3:
				return base.GetInfo(this.LinkDamage, 0);
			case 5:
				return base.GetInfo(this.SnapDamage, 0);
			case 9:
				return base.GetInfo(this.OngoingDamage, 0);
			case 10:
				return base.GetInfo(this.SnapDamage, 0);
			}
			return base.GetInfo(index);
		}

		public float MoveSpeed;

		public string MoveSpeedUpgrade;

		public FXInfo LinkEffect;

		public FXInfo LinkTargetEffect;

		public FXInfo BoostEffect;

		public float SnapDistance;

		public ModifierInfo[] LinkDamage;

		public ModifierInfo[] SnapDamage;

		public ModifierInfo[] OngoingDamage;

		public ModifierInfo[] BoostedDamage;

		public float OngoingRange;

		public string OngoingRangeUpgrade;

		public GadgetSlot OtherLink;

		public float SearchLength;
	}
}
