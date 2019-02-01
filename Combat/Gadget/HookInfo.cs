using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class HookInfo : BasicLinkInfo
	{
		public override Type GadgetType()
		{
			return typeof(Hook);
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
				"Damage 3",
				"Damage 3 Duration",
				"Damage 4",
				"Damage 4 Duration",
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
				"Hook Back Damage 0",
				"Hook Back Damage 0 Duration",
				"Hook Back Damage 1",
				"Hook Back Damage 1 Duration",
				"Hook Back Damage 2",
				"Hook Back Damage 2 Duration",
				"Hook Back Damage 3",
				"Hook Back Damage 3 Duration",
				"Hook Back Damage 4",
				"Hook Back Damage 4 Duration",
				"Hook Back Extra Damage 0",
				"Hook Back Extra Damage 0 Duration",
				"Hook Back Extra Damage 1",
				"Hook Back Extra Damage 1 Duration",
				"Hook Back Extra Damage 2",
				"Hook Back Extra Damage 2 Duration",
				"Hook Back Extra Damage 3",
				"Hook Back Extra Damage 3 Duration",
				"Hook Back Extra Damage 4",
				"Hook Back Extra Damage 4 Duration",
				"Hook Area",
				"Hook Back Move Speed",
				"Move Speed",
				"Cooldown",
				"Activation Cost",
				"Life Time",
				"Extra Life Time",
				"Drain Life",
				"Range",
				"Radius",
				"Life Time Dif"
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
				return base.GetStatListModifierAmount(this.Damage, 3);
			case 7:
				return base.GetStatListModifierLifeTime(this.Damage, 3);
			case 8:
				return base.GetStatListModifierAmount(this.Damage, 4);
			case 9:
				return base.GetStatListModifierLifeTime(this.Damage, 4);
			case 10:
				return base.GetStatListModifierAmount(this.ExtraModifier, 0);
			case 11:
				return base.GetStatListModifierLifeTime(this.ExtraModifier, 0);
			case 12:
				return base.GetStatListModifierAmount(this.ExtraModifier, 1);
			case 13:
				return base.GetStatListModifierLifeTime(this.ExtraModifier, 1);
			case 14:
				return base.GetStatListModifierAmount(this.ExtraModifier, 2);
			case 15:
				return base.GetStatListModifierLifeTime(this.ExtraModifier, 2);
			case 16:
				return base.GetStatListModifierAmount(this.ExtraModifier, 3);
			case 17:
				return base.GetStatListModifierLifeTime(this.ExtraModifier, 3);
			case 18:
				return base.GetStatListModifierAmount(this.ExtraModifier, 4);
			case 19:
				return base.GetStatListModifierLifeTime(this.ExtraModifier, 4);
			case 20:
				return base.GetStatListModifierAmount(this.HookBackDamage, 0);
			case 21:
				return base.GetStatListModifierLifeTime(this.HookBackDamage, 0);
			case 22:
				return base.GetStatListModifierAmount(this.HookBackDamage, 1);
			case 23:
				return base.GetStatListModifierLifeTime(this.HookBackDamage, 1);
			case 24:
				return base.GetStatListModifierAmount(this.HookBackDamage, 2);
			case 25:
				return base.GetStatListModifierLifeTime(this.HookBackDamage, 2);
			case 26:
				return base.GetStatListModifierAmount(this.HookBackDamage, 3);
			case 27:
				return base.GetStatListModifierLifeTime(this.HookBackDamage, 3);
			case 28:
				return base.GetStatListModifierAmount(this.HookBackDamage, 4);
			case 29:
				return base.GetStatListModifierLifeTime(this.HookBackDamage, 4);
			case 30:
				return base.GetStatListModifierAmount(this.HookBackExtraDamage, 0);
			case 31:
				return base.GetStatListModifierLifeTime(this.HookBackExtraDamage, 0);
			case 32:
				return base.GetStatListModifierAmount(this.HookBackExtraDamage, 1);
			case 33:
				return base.GetStatListModifierLifeTime(this.HookBackExtraDamage, 1);
			case 34:
				return base.GetStatListModifierAmount(this.HookBackExtraDamage, 2);
			case 35:
				return base.GetStatListModifierLifeTime(this.HookBackExtraDamage, 2);
			case 36:
				return base.GetStatListModifierAmount(this.HookBackExtraDamage, 3);
			case 37:
				return base.GetStatListModifierLifeTime(this.HookBackExtraDamage, 3);
			case 38:
				return base.GetStatListModifierAmount(this.HookBackExtraDamage, 4);
			case 39:
				return base.GetStatListModifierLifeTime(this.HookBackExtraDamage, 4);
			case 40:
				return base.GetStatListModifier(this.Radius, this.RadiusUpgrade);
			case 41:
				return base.GetStatListSingleValue(this.HookBackMoveSpeed);
			case 42:
				return base.GetStatListModifier(this.MoveSpeed, this.MoveSpeedUpgrade);
			case 43:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 44:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			case 45:
				return base.GetStatListModifier(this.LifeTime, this.LifeTimeUpgrade);
			case 46:
				return base.GetStatListModifier(this.ExtraLifeTime, this.ExtraLifeTimeUpgrade);
			case 47:
				return base.GetStatListModifier(this.DrainLife, this.DrainLifeUpgrade);
			case 48:
				return base.GetStatListModifier(this.Range, this.RangeUpgrade);
			case 49:
				return base.GetStatListModifier(this.Radius, this.RadiusUpgrade);
			case 50:
				return base.GetStatListModifierDif(this.LifeTime, this.LifeTimeUpgrade);
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
			case 4:
				return base.GetInfo(this.HookBackDamage, 0);
			case 5:
				return base.GetInfo(this.HookBackDamage, 0);
			case 9:
				return base.GetInfo(this.HookBackDamage, 1);
			case 10:
				return base.GetInfo(this.HookBackDamage, 1);
			case 11:
				return base.GetInfo(this.HookBackExtraDamage, 0);
			case 12:
				return base.GetInfo(this.HookBackExtraDamage, 0);
			case 13:
				return base.GetInfo(this.HookBackExtraDamage, 1);
			case 14:
				return base.GetInfo(this.HookBackExtraDamage, 1);
			}
			return base.GetInfo(index);
		}

		public FXInfo HookBackEffect;

		public FXInfo HookFailEffect;

		public float HookBackMoveSpeed;

		public float HookFailMoveSpeed;

		public ModifierInfo[] HookBackDamage;

		public ModifierInfo[] HookBackExtraDamage;

		public bool GoToTarget;
	}
}
