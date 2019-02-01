using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class PassiveCreepSpawn : GadgetBehaviour
	{
		public PassiveCreepSpawnInfo MyInfo
		{
			get
			{
				return base.Info as PassiveCreepSpawnInfo;
			}
		}

		public override void SetInfo(GadgetInfo gInfo)
		{
			base.SetInfo(gInfo);
			PassiveCreepSpawnInfo myInfo = this.MyInfo;
			this._maxCreeps = new Upgradeable(myInfo.MaxCreepCountUpgrade, (float)myInfo.MaxCreepCount, myInfo.UpgradesValues);
			this._creepLevel = new Upgradeable(myInfo.CreepLevelUpgrade, (float)myInfo.CreepLevel, myInfo.UpgradesValues);
			if (this._creeps != null)
			{
				this._creeps.Clear();
			}
			this._creeps = new Dictionary<int, CreepInfo>(myInfo.Creeps.Length);
			for (int i = 0; i < myInfo.Creeps.Length; i++)
			{
				PassiveCreepSpawnInfo.LeveledCreep leveledCreep = myInfo.Creeps[i];
				this._creeps[leveledCreep.Level] = leveledCreep.Creep;
				if (leveledCreep.Creep)
				{
					GameHubBehaviour.Hub.Events.Creeps.Factory.CacheCreeps(leveledCreep.Creep, 6);
				}
			}
			GameHubBehaviour.Hub.Events.Creeps.ListenToCreepUnspawn += this.OnCreepUnspawn;
		}

		public override void EditorReset()
		{
			GameHubBehaviour.Hub.Events.Creeps.ListenToCreepUnspawn -= this.OnCreepUnspawn;
			base.EditorReset();
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._maxCreeps.SetLevel(upgradeName, level);
			this._creepLevel.SetLevel(upgradeName, level);
			this._currentCreep = null;
		}

		public override void Activate()
		{
			base.Activate();
			this._currentCreep = null;
			PassiveCreepSpawnInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.CreepSpawnListenerEffect);
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		private CreepInfo GetCurrentCreep()
		{
			if (this._currentCreep)
			{
				return this._currentCreep;
			}
			PassiveCreepSpawnInfo myInfo = this.MyInfo;
			if (!base.Activated || !this._creeps.TryGetValue(this._creepLevel.IntGet(), out this._currentCreep))
			{
				this._currentCreep = myInfo.BaseCreep;
			}
			return this._currentCreep;
		}

		protected override void GadgetUpdate()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (!base.Activated)
			{
				return;
			}
			this.CurrentTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			base.GadgetUpdate();
			if (this.CurrentCooldownTime > this.CurrentTime)
			{
				return;
			}
			if (!this.Combat.IsAlive())
			{
				return;
			}
			if (this._creepList.Count >= ((!base.Activated) ? this.MyInfo.BaseMaxCreeps : this._maxCreeps.IntGet()))
			{
				return;
			}
			this.CurrentTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this.CurrentCooldownTime = (long)(this.Cooldown * 1000f) + this.CurrentTime;
			this.SpawnCreep(this.Combat.Transform.position);
		}

		public void SpawnCreep(Vector3 position)
		{
			if (this._creepList.Count >= ((!base.Activated) ? this.MyInfo.BaseMaxCreeps : this._maxCreeps.IntGet()))
			{
				return;
			}
			CreepInfo currentCreep = this.GetCurrentCreep();
			int creepId = GameHubBehaviour.Hub.Events.Creeps.GetCreepId();
			this._creepList.Add(creepId);
			CreepEvent content = new CreepEvent
			{
				CauserId = this.Combat.Id.ObjId,
				CreepId = creepId,
				CreepInfoId = currentCreep.CreepInfoId,
				Location = position,
				Direction = base.CalcDirection(this.Combat.Transform.position, this.Combat.Transform.position + this.Combat.Transform.forward),
				Reason = SpawnReason.Respawn,
				CreepTeam = this.Combat.Team,
				Level = this._creepLevel.IntGet() + 1
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
		}

		private void OnCreepUnspawn(CreepRemoveEvent data)
		{
			int creepId = data.CreepId;
			if (!this._creepList.Contains(creepId))
			{
				return;
			}
			this._creepList.Remove(creepId);
			this._creepCache.Remove(creepId);
		}

		public void BuffedAggroCreeps(int targetId, float lifetime, ModifierData[] creepBuffs, CombatObject causer, int eventId)
		{
			for (int i = 0; i < this._creepList.Count; i++)
			{
				int num = this._creepList[i];
				Identifiable @object;
				if (!this._creepCache.TryGetValue(num, out @object))
				{
					@object = GameHubBehaviour.Hub.ObjectCollection.GetObject(num);
					if (@object)
					{
						this._creepCache[num] = @object;
					}
				}
				if (@object)
				{
					CombatObject component = @object.GetComponent<CombatObject>();
					CreepController component2 = @object.GetComponent<CreepController>();
					if (component)
					{
						component.Controller.AddModifiers(creepBuffs, causer, eventId, false);
					}
					if (component2)
					{
						Identifiable object2 = GameHubBehaviour.Hub.ObjectCollection.GetObject(targetId);
						CombatObject combatObject = (!object2) ? null : object2.GetComponent<CombatObject>();
						if (combatObject)
						{
							component2.SetCurrentTarget(combatObject, lifetime);
						}
					}
				}
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PassiveCreepSpawn));

		private Upgradeable _maxCreeps;

		private Upgradeable _creepLevel;

		private Dictionary<int, CreepInfo> _creeps;

		private CreepInfo _currentCreep;

		private List<int> _creepList = new List<int>();

		private Dictionary<int, Identifiable> _creepCache = new Dictionary<int, Identifiable>();
	}
}
