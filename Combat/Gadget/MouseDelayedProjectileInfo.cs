using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class MouseDelayedProjectileInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(MouseDelayedProjectile);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Damage 0",
				"Damage 0 Dif",
				"Damage 0 Duration",
				"Damage 0 DurationDif",
				"Damage 1",
				"Damage 1 Dif",
				"Damage 1 Duration",
				"Damage 1 Duration Dif",
				"Damage 2",
				"Damage 2 Dif",
				"Damage 2 Duration",
				"Damage 2 Duration Dif",
				"Damage 3",
				"Damage 3 Dif",
				"Damage 3 Duration",
				"Damage 3 Duration Dif",
				"Damage 4",
				"Damage 4 Dif",
				"Damage 4 Duration",
				"Damage 4 Duration Dif",
				"Extra Modifier 0",
				"Extra Modifier 0 Dif",
				"Extra Modifier 0 Duration",
				"Extra Modifier 0 Duration Dif",
				"Extra Modifier 1",
				"Extra Modifier 1 Dif",
				"Extra Modifier 1 Duration",
				"Extra Modifier 1 Duration Dif",
				"Extra Modifier 2",
				"Extra Modifier 2 Dif",
				"Extra Modifier 2 Duration",
				"Extra Modifier 2 Duration Dif",
				"Extra Modifier 3",
				"Extra Modifier 3 Dif",
				"Extra Modifier 3 Duration",
				"Extra Modifier 3 Duration Dif",
				"Extra Modifier 4",
				"Extra Modifier 4 Dif",
				"Extra Modifier 4 Duration",
				"Extra Modifier 4 Duration Dif",
				"Extra Life Time",
				"Extra Life Time Dif",
				"Damage 0",
				"Damage 0 Dif",
				"Damage 0 Duration",
				"Damage 0 Duration Dif",
				"Damage 0 DPS",
				"Damage 0 DPS Dif",
				"Damage 1",
				"Damage 1 Dif",
				"Damage 1 Duration",
				"Damage 1 Duration Dif",
				"Damage 1 DPS",
				"Damage 1 DPS Dif",
				"Damage 2 ",
				"Damage 2 Dif",
				"Damage 2 Duration",
				"Damage 2 Duration Dif",
				"Damage 2 DPS",
				"Damage 2 DPS Dif",
				"Damage 3",
				"Damage 3 Dif",
				"Damage 3 Duration",
				"Damage 3 Duration Dif",
				"Damage 3 DPS",
				"Damage 3 DPS Dif",
				"Damage 4",
				"Damage 4 Dif",
				"Damage 4 Duration",
				"Damage 4 Duration Dif",
				"Damage 4 DPS",
				"Damage 4 DPS Dif",
				"Damage 5",
				"Damage 5 Dif",
				"Damage 5 Duration",
				"Damage 5 Duration Dif",
				"Damage 5 DPS",
				"Damage 5 DPS Dif",
				"Projectile Move Speed",
				"Projectile Move Speed Dif",
				"Cooldown",
				"Cooldown Dif",
				"Activation Cost",
				"Activation Cost Dif",
				"Life Time",
				"Life Time Dif",
				"Drain Life",
				"Drain Life Dif",
				"Range",
				"Range Dif",
				"Radius",
				"Radius Dif",
				"Warmup Seconds",
				"Extra Warmup Seconds"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifierAmount(this.Damage, 0);
			case 1:
				return base.GetStatListModifierAmountDif(this.Damage, 0);
			case 2:
				return base.GetStatListModifierLifeTime(this.Damage, 0);
			case 3:
				return base.GetStatListModifierLifeTimeDif(this.Damage, 0);
			case 4:
				return base.GetStatListModifierAmount(this.Damage, 1);
			case 5:
				return base.GetStatListModifierAmountDif(this.Damage, 1);
			case 6:
				return base.GetStatListModifierLifeTime(this.Damage, 1);
			case 7:
				return base.GetStatListModifierLifeTimeDif(this.Damage, 1);
			case 8:
				return base.GetStatListModifierAmount(this.Damage, 2);
			case 9:
				return base.GetStatListModifierAmountDif(this.Damage, 2);
			case 10:
				return base.GetStatListModifierLifeTime(this.Damage, 2);
			case 11:
				return base.GetStatListModifierLifeTimeDif(this.Damage, 2);
			case 12:
				return base.GetStatListModifierAmount(this.Damage, 3);
			case 13:
				return base.GetStatListModifierAmountDif(this.Damage, 3);
			case 14:
				return base.GetStatListModifierLifeTime(this.Damage, 3);
			case 15:
				return base.GetStatListModifierLifeTimeDif(this.Damage, 3);
			case 16:
				return base.GetStatListModifierAmount(this.Damage, 4);
			case 17:
				return base.GetStatListModifierAmountDif(this.Damage, 4);
			case 18:
				return base.GetStatListModifierLifeTime(this.Damage, 4);
			case 19:
				return base.GetStatListModifierLifeTimeDif(this.Damage, 4);
			case 20:
				return base.GetStatListModifierAmount(this.ExtraModifier, 0);
			case 21:
				return base.GetStatListModifierAmountDif(this.ExtraModifier, 0);
			case 22:
				return base.GetStatListModifierLifeTime(this.ExtraModifier, 0);
			case 23:
				return base.GetStatListModifierLifeTimeDif(this.ExtraModifier, 0);
			case 24:
				return base.GetStatListModifierAmount(this.ExtraModifier, 1);
			case 25:
				return base.GetStatListModifierAmountDif(this.ExtraModifier, 1);
			case 26:
				return base.GetStatListModifierLifeTime(this.ExtraModifier, 1);
			case 27:
				return base.GetStatListModifierLifeTimeDif(this.ExtraModifier, 1);
			case 28:
				return base.GetStatListModifierAmount(this.ExtraModifier, 2);
			case 29:
				return base.GetStatListModifierAmountDif(this.ExtraModifier, 2);
			case 30:
				return base.GetStatListModifierLifeTime(this.ExtraModifier, 2);
			case 31:
				return base.GetStatListModifierLifeTimeDif(this.ExtraModifier, 2);
			case 32:
				return base.GetStatListModifierAmount(this.ExtraModifier, 3);
			case 33:
				return base.GetStatListModifierAmountDif(this.ExtraModifier, 3);
			case 34:
				return base.GetStatListModifierLifeTime(this.ExtraModifier, 3);
			case 35:
				return base.GetStatListModifierLifeTimeDif(this.ExtraModifier, 3);
			case 36:
				return base.GetStatListModifierAmount(this.ExtraModifier, 4);
			case 37:
				return base.GetStatListModifierAmountDif(this.ExtraModifier, 4);
			case 38:
				return base.GetStatListModifierLifeTime(this.ExtraModifier, 4);
			case 39:
				return base.GetStatListModifierLifeTimeDif(this.ExtraModifier, 4);
			case 40:
				return base.GetStatListModifier(this.ExtraLifeTime, this.ExtraLifeTimeUpgrade);
			case 41:
				return base.GetStatListModifierDif(this.ExtraLifeTime, this.ExtraLifeTimeUpgrade);
			case 42:
				return base.GetStatListModifierAmount(this.Damage, 0);
			case 43:
				return base.GetStatListModifierAmountDif(this.Damage, 0);
			case 44:
				return base.GetStatListModifierLifeTime(this.Damage, 0);
			case 45:
				return base.GetStatListModifierLifeTimeDif(this.Damage, 0);
			case 46:
				return base.GetStatListModifierAmountPerSecond(this.Damage, 0);
			case 47:
				return base.GetStatListModifierAmountPerSecondDif(this.Damage, 0);
			case 48:
				return base.GetStatListModifierAmount(this.Damage, 1);
			case 49:
				return base.GetStatListModifierAmountDif(this.Damage, 1);
			case 50:
				return base.GetStatListModifierLifeTime(this.Damage, 1);
			case 51:
				return base.GetStatListModifierLifeTimeDif(this.Damage, 1);
			case 52:
				return base.GetStatListModifierAmountPerSecond(this.Damage, 1);
			case 53:
				return base.GetStatListModifierAmountPerSecondDif(this.Damage, 1);
			case 54:
				return base.GetStatListModifierAmount(this.Damage, 2);
			case 55:
				return base.GetStatListModifierAmountDif(this.Damage, 2);
			case 56:
				return base.GetStatListModifierLifeTime(this.Damage, 2);
			case 57:
				return base.GetStatListModifierLifeTimeDif(this.Damage, 2);
			case 58:
				return base.GetStatListModifierAmountPerSecond(this.Damage, 2);
			case 59:
				return base.GetStatListModifierAmountPerSecondDif(this.Damage, 2);
			case 60:
				return base.GetStatListModifierAmount(this.Damage, 3);
			case 61:
				return base.GetStatListModifierAmountDif(this.Damage, 3);
			case 62:
				return base.GetStatListModifierLifeTime(this.Damage, 3);
			case 63:
				return base.GetStatListModifierLifeTimeDif(this.Damage, 3);
			case 64:
				return base.GetStatListModifierAmountPerSecond(this.Damage, 3);
			case 65:
				return base.GetStatListModifierAmountPerSecondDif(this.Damage, 3);
			case 66:
				return base.GetStatListModifierAmount(this.Damage, 4);
			case 67:
				return base.GetStatListModifierAmountDif(this.Damage, 4);
			case 68:
				return base.GetStatListModifierLifeTime(this.Damage, 4);
			case 69:
				return base.GetStatListModifierLifeTimeDif(this.Damage, 4);
			case 70:
				return base.GetStatListModifierAmountPerSecond(this.Damage, 4);
			case 71:
				return base.GetStatListModifierAmountPerSecondDif(this.Damage, 4);
			case 72:
				return base.GetStatListModifierAmount(this.Damage, 5);
			case 73:
				return base.GetStatListModifierAmountDif(this.Damage, 5);
			case 74:
				return base.GetStatListModifierLifeTime(this.Damage, 5);
			case 75:
				return base.GetStatListModifierLifeTimeDif(this.Damage, 5);
			case 76:
				return base.GetStatListModifierAmountPerSecond(this.Damage, 5);
			case 77:
				return base.GetStatListModifierAmountPerSecondDif(this.Damage, 5);
			case 78:
				return base.GetStatListModifier(this.MoveSpeed, this.MoveSpeedUpgrade);
			case 79:
				return base.GetStatListModifierDif(this.MoveSpeed, this.MoveSpeedUpgrade);
			case 80:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 81:
				return base.GetStatListModifierDif(this.Cooldown, this.CooldownUpgrade);
			case 82:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			case 83:
				return base.GetStatListModifierDif((float)this.ActivationCost, this.ActivationCostUpgrade);
			case 84:
				return base.GetStatListModifier(this.LifeTime, this.LifeTimeUpgrade);
			case 85:
				return base.GetStatListModifierDif(this.LifeTime, this.LifeTimeUpgrade);
			case 86:
				return base.GetStatListModifier(this.DrainLife, this.DrainLifeUpgrade);
			case 87:
				return base.GetStatListModifierDif(this.DrainLife, this.DrainLifeUpgrade);
			case 88:
				return base.GetStatListModifier(this.Range, this.RangeUpgrade);
			case 89:
				return base.GetStatListModifierDif(this.Range, this.RangeUpgrade);
			case 90:
				return base.GetStatListModifier(this.Radius, this.RadiusUpgrade);
			case 91:
				return base.GetStatListModifierDif(this.Radius, this.RadiusUpgrade);
			case 92:
				return base.GetStatListSingleValue(this.WarmupSeconds);
			case 93:
				return base.GetStatListSingleValue(this.ExtraWarmupSeconds);
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

		public bool ReEvaluateTargetOnFire;

		public float ExtraWarmupSeconds;

		public FXInfo ExtraWarmupEffect;
	}
}
