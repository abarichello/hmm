using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class PassiveCreepSpawnInfo : GadgetInfo
	{
		public override Type GadgetType()
		{
			return typeof(PassiveCreepSpawn);
		}

		public override string[] GetStatStrings()
		{
			return new string[]
			{
				"Max",
				"HP",
				"Damage",
				"Cooldown"
			};
		}

		public override List<float> GetStats(int index)
		{
			switch (index)
			{
			case 0:
				return base.GetStatListModifier((float)this.MaxCreepCount, this.MaxCreepCountUpgrade);
			case 1:
				return this.CreepsHP();
			case 2:
				return this.CreepsDamage();
			case 3:
				return base.GetStatListModifier(this.Cooldown, this.CooldownUpgrade);
			default:
				return base.GetStats(index);
			}
		}

		private List<float> CreepsHP()
		{
			List<float> list = new List<float>(this.Creeps.Length);
			for (int i = 0; i < this.Creeps.Length; i++)
			{
				list.Add(0f);
			}
			for (int j = 0; j < this.Creeps.Length; j++)
			{
				list[this.Creeps[j].Level] = (float)this.Creeps[j].Creep.GetCombatInfo().HPMax;
			}
			return list;
		}

		private List<float> CreepsDamage()
		{
			List<float> list = new List<float>(this.Creeps.Length);
			for (int i = 0; i < this.Creeps.Length; i++)
			{
				list.Add(0f);
			}
			for (int j = 0; j < this.Creeps.Length; j++)
			{
				if (!(this.Creeps[j].Creep.Attack is BasicAttackInfo))
				{
					list[this.Creeps[j].Level] = 0f;
				}
				else
				{
					BasicAttackInfo basicAttackInfo = (BasicAttackInfo)this.Creeps[j].Creep.Attack;
					list[this.Creeps[j].Level] = basicAttackInfo.Damage.Amount;
				}
			}
			return list;
		}

		public int MaxCreepCount;

		public string MaxCreepCountUpgrade;

		public PassiveCreepSpawnInfo.LeveledCreep[] Creeps;

		public int CreepLevel;

		public string CreepLevelUpgrade;

		public CreepInfo BaseCreep;

		public int BaseMaxCreeps;

		public FXInfo CreepSpawnListenerEffect;

		[Serializable]
		public class LeveledCreep
		{
			public int Level;

			public CreepInfo Creep;
		}
	}
}
