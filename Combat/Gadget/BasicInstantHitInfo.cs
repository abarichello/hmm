using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Obsolete]
	[Serializable]
	public class BasicInstantHitInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(BasicInstantHit);
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
				"Range",
				"Cooldown",
				"EP",
				"Self Damage 0",
				"Self Damage 0 Duration",
				"Self Damage 1",
				"Self Damage 1 Duration",
				"Self Damage 2",
				"Self Damage 2 Duration"
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
				return base.GetStatListModifier(this.Range, this.RangeUpgrade);
			case 14:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 15:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			case 16:
				return base.GetStatListModifierAmount(this.SelfDamage, 0);
			case 17:
				return base.GetStatListModifierLifeTime(this.SelfDamage, 0);
			case 18:
				return base.GetStatListModifierAmount(this.SelfDamage, 1);
			case 19:
				return base.GetStatListModifierLifeTime(this.SelfDamage, 1);
			case 20:
				return base.GetStatListModifierAmount(this.SelfDamage, 2);
			case 21:
				return base.GetStatListModifierLifeTime(this.SelfDamage, 2);
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

		[Header("THIS TYPE IS OBSOLETE!! TALK WITH A DEV IF YOU WANT TO USE IT!!!!")]
		public float MoveSpeed;

		public string MoveSpeedUpgrade;

		public ModifierInfo[] SelfDamage;

		public float CasterMaxHealthThreshold;

		public CombatCheckHitData HitMask;
	}
}
