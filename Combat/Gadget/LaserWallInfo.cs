using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class LaserWallInfo : MortarEffectInfo
	{
		public override Type GadgetType()
		{
			return typeof(LaserWall);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Amount",
				"Amount x2",
				"Player Damage 0",
				"Player Damage 0 Duration",
				"Duration",
				"Cooldown",
				"EP",
				"Player Damage 1",
				"Player Damage 1 Duration",
				"Player Damage 2",
				"Player Damage 2 Duration",
				"Player Damage 3",
				"Player Damage 3 Duration",
				"Player Damage 4",
				"Player Damage 4 Duration",
				"Player Damage 4 x2"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifier((float)this.WallAmount, this.WallAmountUpgrade);
			case 1:
				return base.GetStatListModifierConvoluted((float)this.WallAmount, this.WallAmountUpgrade, 2f);
			case 2:
				return base.GetStatListModifierAmount(this.PlayerDamage, 0);
			case 3:
				return base.GetStatListModifierLifeTime(this.PlayerDamage, 0);
			case 4:
				return base.GetStatListModifier(this.WallLifeTime, this.WallLifeTimeUpgrade);
			case 5:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 6:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			case 7:
				return base.GetStatListModifierAmount(this.PlayerDamage, 1);
			case 8:
				return base.GetStatListModifierLifeTime(this.PlayerDamage, 1);
			case 9:
				return base.GetStatListModifierAmount(this.PlayerDamage, 2);
			case 10:
				return base.GetStatListModifierLifeTime(this.PlayerDamage, 2);
			case 11:
				return base.GetStatListModifierAmount(this.PlayerDamage, 3);
			case 12:
				return base.GetStatListModifierLifeTime(this.PlayerDamage, 3);
			case 13:
				return base.GetStatListModifierAmount(this.PlayerDamage, 4);
			case 14:
				return base.GetStatListModifierLifeTime(this.PlayerDamage, 4);
			case 15:
				return base.GetStatListModifierAmountConvoluted(this.PlayerDamage, 4, 2f);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			switch (index)
			{
			case 2:
				return base.GetInfo(this.PlayerDamage, 0);
			case 3:
				return base.GetInfo(this.PlayerDamage, 0);
			case 7:
				return base.GetInfo(this.PlayerDamage, 1);
			case 8:
				return base.GetInfo(this.PlayerDamage, 1);
			case 9:
				return base.GetInfo(this.PlayerDamage, 2);
			case 10:
				return base.GetInfo(this.PlayerDamage, 2);
			case 11:
				return base.GetInfo(this.PlayerDamage, 3);
			case 12:
				return base.GetInfo(this.PlayerDamage, 3);
			case 13:
				return base.GetInfo(this.PlayerDamage, 4);
			case 14:
				return base.GetInfo(this.PlayerDamage, 4);
			case 15:
				return base.GetInfo(this.PlayerDamage, 4);
			}
			return base.GetInfo(index);
		}

		public float WallLifeTime;

		public string WallLifeTimeUpgrade;

		public int WallAmount;

		public string WallAmountUpgrade;

		public ModifierInfo[] PlayerDamage;

		public FXInfo PlayerEffect;

		public float PlayerRadius;

		public FXInfo ProjectileEffect;
	}
}
