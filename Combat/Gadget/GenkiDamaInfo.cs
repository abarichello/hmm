using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class GenkiDamaInfo : BasicCannonInfo
	{
		public override Type GadgetType()
		{
			return typeof(GenkiDama);
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
				"Trail Modifier 0",
				"Trail Modifier 0 Duration",
				"Trail Modifier 1",
				"Trail Modifier 1 Duration",
				"Trail Modifier 2",
				"Trail Modifier 2 Duration",
				"Trail Modifier 3",
				"Trail Modifier 3 Duration",
				"Trail Modifier 4",
				"Trail Modifier 4 Duration",
				"Trail Pieces Life Time",
				"Trail Pieces Drop Interval Millis",
				"Trail Collider Radius",
				"On Destroy Modifier 0",
				"On Destroy Modifier 0 Duration",
				"On Destroy Modifier 1",
				"On Destroy Modifier 1 Duration",
				"On Destroy Modifier 2",
				"On Destroy Modifier 2 Duration",
				"On Destroy Modifier 3",
				"On Destroy Modifier 3 Duration",
				"On Destroy Modifier 4",
				"On Destroy Modifier 4 Duration",
				"On Destroy Effect Life Time",
				"Move Speed",
				"Cooldown",
				"Activation Cost",
				"Life Time",
				"Extra Life Time",
				"Drain Life",
				"Range",
				"Radius",
				"Damage 2 Dif",
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
				return base.GetStatListModifierAmount(this.TrailModifier, 0);
			case 24:
				return base.GetStatListModifierLifeTime(this.TrailModifier, 0);
			case 25:
				return base.GetStatListModifierAmount(this.TrailModifier, 1);
			case 26:
				return base.GetStatListModifierLifeTime(this.TrailModifier, 1);
			case 27:
				return base.GetStatListModifierAmount(this.TrailModifier, 2);
			case 28:
				return base.GetStatListModifierLifeTime(this.TrailModifier, 2);
			case 29:
				return base.GetStatListModifierAmount(this.TrailModifier, 3);
			case 30:
				return base.GetStatListModifierLifeTime(this.TrailModifier, 3);
			case 31:
				return base.GetStatListModifierAmount(this.TrailModifier, 4);
			case 32:
				return base.GetStatListModifierLifeTime(this.TrailModifier, 4);
			case 33:
				return base.GetStatListSingleValue(this.TrailPiecesLifeTime);
			case 34:
				return base.GetStatListSingleValue(this.TrailPiecesDropIntervalMillis);
			case 35:
				return base.GetStatListSingleValue((float)this.TrailColliderRadius);
			case 36:
				return base.GetStatListModifierAmount(this.OnDestroyEffectModifier, 0);
			case 37:
				return base.GetStatListModifierLifeTime(this.OnDestroyEffectModifier, 0);
			case 38:
				return base.GetStatListModifierAmount(this.OnDestroyEffectModifier, 1);
			case 39:
				return base.GetStatListModifierLifeTime(this.OnDestroyEffectModifier, 1);
			case 40:
				return base.GetStatListModifierAmount(this.OnDestroyEffectModifier, 2);
			case 41:
				return base.GetStatListModifierLifeTime(this.OnDestroyEffectModifier, 2);
			case 42:
				return base.GetStatListModifierAmount(this.OnDestroyEffectModifier, 3);
			case 43:
				return base.GetStatListModifierLifeTime(this.OnDestroyEffectModifier, 3);
			case 44:
				return base.GetStatListModifierAmount(this.OnDestroyEffectModifier, 4);
			case 45:
				return base.GetStatListModifierLifeTime(this.OnDestroyEffectModifier, 4);
			case 46:
				return base.GetStatListSingleValue(this.OnDestroyEffectLifeTime);
			case 47:
				return base.GetStatListModifier(this.MoveSpeed, this.MoveSpeedUpgrade);
			case 48:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 49:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			case 50:
				return base.GetStatListModifier(this.LifeTime, this.LifeTimeUpgrade);
			case 51:
				return base.GetStatListModifier(this.ExtraLifeTime, this.ExtraLifeTimeUpgrade);
			case 52:
				return base.GetStatListModifier(this.DrainLife, this.DrainLifeUpgrade);
			case 53:
				return base.GetStatListModifier(this.Range, this.RangeUpgrade);
			case 54:
				return base.GetStatListModifier(this.Radius, this.RadiusUpgrade);
			case 55:
				return base.GetStatListModifierAmountDif(this.Damage, 2);
			case 56:
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
				return base.GetInfo(this.Damage, 2);
			case 5:
				return base.GetInfo(this.Damage, 2);
			default:
				return base.GetInfo(index);
			}
		}

		public int EffectsToSpawnCount;

		[Header("##### Must be less than 255 #####")]
		public int ColliderRadius;

		public string ColliderRadiusUpgrade;

		public bool TrailMustFollowCar;

		public FXInfo TrailEffect;

		public ModifierInfo[] TrailModifier;

		public float TrailPiecesLifeTime;

		public float TrailPiecesDropIntervalMillis;

		public int TrailColliderRadius;

		public FXInfo OnDestroyEffect;

		public ModifierInfo[] OnDestroyEffectModifier;

		public float OnDestroyEffectLifeTime;

		public int OnDestroyEffectColliderRadius;

		public bool DestroyOnHitPlayer;

		public string DestroyOnHitPlayerUpgrade;
	}
}
