using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class DirtDevilStayingHookInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(DirtDevilStayingHook);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Go Damage 0",
				"Go Damage 0 Duration",
				"Go Damage 1",
				"Go Damage 1 Duration",
				"Back Damage 0",
				"Back Damage 0 Duration",
				"Go Range",
				"Cooldown",
				"EP",
				"Back Damage 1",
				"Back Damage 1 Duration",
				"Back Extra Damage 0",
				"Back Extra Damage 0 Duration",
				"Back Extra Damage 1",
				"Back Extra Damage 1 Duration",
				"Back Extra Damage 2",
				"Back Extra Damage 2 Duration"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifierAmount(this.HookGoDamage, 0);
			case 1:
				return base.GetStatListModifierLifeTime(this.HookGoDamage, 0);
			case 2:
				return base.GetStatListModifierAmount(this.HookGoDamage, 1);
			case 3:
				return base.GetStatListModifierLifeTime(this.HookGoDamage, 1);
			case 4:
				return base.GetStatListModifierAmount(this.HookStayAndBackDamage, 0);
			case 5:
				return base.GetStatListModifierLifeTime(this.HookStayAndBackDamage, 0);
			case 6:
				return base.GetStatListSingleValue(this.HookGoRange);
			case 7:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 8:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			case 9:
				return base.GetStatListModifierAmount(this.HookStayAndBackDamage, 1);
			case 10:
				return base.GetStatListModifierLifeTime(this.HookStayAndBackDamage, 1);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetInfo(this.HookGoDamage, 0);
			case 1:
				return base.GetInfo(this.HookGoDamage, 0);
			case 2:
				return base.GetInfo(this.HookGoDamage, 1);
			case 3:
				return base.GetInfo(this.HookGoDamage, 1);
			case 4:
				return base.GetInfo(this.HookStayAndBackDamage, 0);
			case 5:
				return base.GetInfo(this.HookStayAndBackDamage, 0);
			case 9:
				return base.GetInfo(this.HookStayAndBackDamage, 1);
			case 10:
				return base.GetInfo(this.HookStayAndBackDamage, 1);
			}
			return base.GetInfo(index);
		}

		public FXInfo HookGoEffect;

		public ModifierInfo[] HookGoDamage;

		public float HookGoSpeed;

		public float HookGoRange;

		public FXInfo HookFailEffect;

		public float HookFailMoveSpeed;

		public FXInfo HookStayEffect;

		public FXInfo HookStayWallEffect;

		public float HookStayRange;

		public float HookStayRangeWall;

		public float HookStayLifeTime;

		public float HookStayWallLifeTime;

		public FXInfo HookBackEffect;

		public FXInfo HookBackWallEffect;

		public FXInfo HookBackOwnerEffect;

		public ModifierInfo[] HookBackDamage;

		public ModifierInfo[] HookStayAndBackDamage;

		public float HookBackMoveSpeed;
	}
}
