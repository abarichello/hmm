using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class LadyMuerteCreepBeaconInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(LadyMuerteCreepBeacon);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"ProjectileModifiers 0",
				"ProjectileModifiers 0 Duration",
				"ProjectileModifiers 1",
				"ProjectileModifiers 1 Duration",
				"ProjectileModifiers 2",
				"ProjectileModifiers 2 Duration",
				"CreepBuffModifiers 0",
				"CreepBuffModifiers 0 Duration",
				"CreepMax",
				"CreepHp",
				"CreepDamage",
				"CreepRespawn",
				"Cooldown",
				"EP",
				"ProjectileModifiers 3",
				"ProjectileModifiers 3 Duration",
				"ProjectileModifiers 4",
				"ProjectileModifiers 4 Duration"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifierAmount(this.ProjectileModifiers, 0);
			case 1:
				return base.GetStatListModifierLifeTime(this.ProjectileModifiers, 0);
			case 2:
				return base.GetStatListModifierAmount(this.ProjectileModifiers, 1);
			case 3:
				return base.GetStatListModifierLifeTime(this.ProjectileModifiers, 1);
			case 4:
				return base.GetStatListModifierAmount(this.ProjectileModifiers, 2);
			case 5:
				return base.GetStatListModifierLifeTime(this.ProjectileModifiers, 2);
			case 6:
				return base.GetStatListModifierAmount(this.CreepBuffModifiers, 0);
			case 7:
				return base.GetStatListModifierLifeTime(this.CreepBuffModifiers, 0);
			case 8:
				return base.GetStatListModifier((float)this.CreepMax, this.CreepMaxUpgrade);
			case 9:
				return this.CreepsHP();
			case 10:
				return this.CreepsDamage();
			case 11:
				return base.GetStatListModifier(this.CreepRespawn, this.CreepRespawnUpgrade);
			case 12:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			case 13:
				return base.GetStatListModifier((float)this.ActivationCost, this.ActivationCostUpgrade);
			case 14:
				return base.GetStatListModifierAmount(this.ProjectileModifiers, 3);
			case 15:
				return base.GetStatListModifierLifeTime(this.ProjectileModifiers, 3);
			case 16:
				return base.GetStatListModifierAmount(this.ProjectileModifiers, 4);
			case 17:
				return base.GetStatListModifierLifeTime(this.ProjectileModifiers, 4);
			default:
				return base.GetStats(index);
			}
		}

		public override ModifierInfo GetInfo(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetInfo(this.ProjectileModifiers, 0);
			case 1:
				return base.GetInfo(this.ProjectileModifiers, 0);
			case 2:
				return base.GetInfo(this.ProjectileModifiers, 1);
			case 3:
				return base.GetInfo(this.ProjectileModifiers, 1);
			case 4:
				return base.GetInfo(this.ProjectileModifiers, 2);
			case 5:
				return base.GetInfo(this.ProjectileModifiers, 2);
			case 6:
				return base.GetInfo(this.CreepBuffModifiers, 0);
			case 7:
				return base.GetInfo(this.CreepBuffModifiers, 0);
			default:
				return base.GetInfo(index);
			}
		}

		private List<float> CreepsHP()
		{
			List<float> list = new List<float>(this.CreepInfos.Length);
			for (int i = 0; i < this.CreepInfos.Length; i++)
			{
				list.Add(0f);
			}
			for (int j = 0; j < this.CreepInfos.Length; j++)
			{
				list[this.CreepInfos[j].Level] = (float)this.CreepInfos[j].Creep.GetCombatInfo().HPMax;
			}
			return list;
		}

		private List<float> CreepsDamage()
		{
			List<float> list = new List<float>(this.CreepInfos.Length);
			for (int i = 0; i < this.CreepInfos.Length; i++)
			{
				list.Add(0f);
			}
			for (int j = 0; j < this.CreepInfos.Length; j++)
			{
				if (!(this.CreepInfos[j].Creep.Attack is BasicAttackInfo))
				{
					list[this.CreepInfos[j].Level] = 0f;
				}
				else
				{
					BasicAttackInfo basicAttackInfo = (BasicAttackInfo)this.CreepInfos[j].Creep.Attack;
					list[this.CreepInfos[j].Level] = basicAttackInfo.Damage.Amount;
				}
			}
			return list;
		}

		public ModifierInfo[] ProjectileModifiers;

		public FXInfo ProjectileEffect;

		public float ProjectileMoveSpeed;

		public float ProjectileRange;

		public ModifierInfo[] CreepBuffModifiers;

		public float CreepBuffLifeTime;

		public LadyMuerteCreepBeaconInfo.LeveledCreep[] CreepInfos;

		public int CreepMax;

		public string CreepMaxUpgrade;

		public int CreepLevel;

		public string CreepLevelUpgrade;

		public float CreepRespawn;

		public string CreepRespawnUpgrade;

		public CreepInfo BaseCreepInfo;

		public int BaseCreepMax;

		[Serializable]
		public class LeveledCreep
		{
			public int Level;

			public CreepInfo Creep;
		}
	}
}
