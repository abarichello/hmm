using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;
using UnityEngine.AI;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class LadyMuerteCreepBeacon : GadgetBehaviour
	{
		public LadyMuerteCreepBeaconInfo MyInfo
		{
			get
			{
				return base.Info as LadyMuerteCreepBeaconInfo;
			}
		}

		public override void SetInfo(GadgetInfo gInfo)
		{
			base.SetInfo(gInfo);
			this._upgProjectileModifiers = ModifierData.CreateData(this.MyInfo.ProjectileModifiers, this.MyInfo);
			this._upgCreepBuffModifiers = ModifierData.CreateData(this.MyInfo.CreepBuffModifiers, this.MyInfo);
			this._upgCreepMax = new Upgradeable(this.MyInfo.CreepMaxUpgrade, (float)this.MyInfo.CreepMax, this.MyInfo.UpgradesValues);
			this._upgCreepLevel = new Upgradeable(this.MyInfo.CreepLevelUpgrade, (float)this.MyInfo.CreepLevel, this.MyInfo.UpgradesValues);
			if (this._creepInfos != null)
			{
				this._creepInfos.Clear();
			}
			this._creepInfos = new Dictionary<int, CreepInfo>(this.MyInfo.CreepInfos.Length);
			for (int i = 0; i < this.MyInfo.CreepInfos.Length; i++)
			{
				LadyMuerteCreepBeaconInfo.LeveledCreep leveledCreep = this.MyInfo.CreepInfos[i];
				this._creepInfos[leveledCreep.Level] = leveledCreep.Creep;
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
			this._upgProjectileModifiers.SetLevel(upgradeName, level);
			this._upgCreepBuffModifiers.SetLevel(upgradeName, level);
			this._upgCreepMax.SetLevel(upgradeName, level);
			this._upgCreepLevel.SetLevel(upgradeName, level);
		}

		private void UpdateCreepCount()
		{
			this.ChargeCount = this._creepInstances.Count;
		}

		protected override void GadgetUpdate()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.CurrentTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			base.GadgetUpdate();
			if (this.CreepMaxReached)
			{
				this._nextCreepRespawnMillis = 0L;
				this.ChargeTime = 0L;
			}
			else if (this._nextCreepRespawnMillis <= this.CurrentTime)
			{
				if (this._nextCreepRespawnMillis == 0L)
				{
					long currentTime = this.CurrentTime;
					long num = currentTime + (long)(base.MaxChargeTime * 1000f);
					this._nextCreepRespawnMillis = num;
					this.ChargeTime = num;
				}
				else
				{
					this.SpawnCreep(this.Combat.Transform.position);
					if (!this.CreepMaxReached)
					{
						long nextCreepRespawnMillis = this._nextCreepRespawnMillis;
						long num2 = nextCreepRespawnMillis + (long)(base.MaxChargeTime * 1000f);
						this._nextCreepRespawnMillis = num2;
						this.ChargeTime = num2;
					}
				}
			}
			if (this.CurrentCooldownTime > this.CurrentTime)
			{
				return;
			}
			if (!base.Pressed)
			{
				this.CurrentCooldownTime = this.CurrentTime;
				return;
			}
			if (!this.Combat.Controller.ConsumeEP((float)base.ActivationCost))
			{
				return;
			}
			long num3 = this.CurrentTime - this.CurrentCooldownTime;
			this.CurrentCooldownTime = (long)(this.Cooldown * 1000f) + this.CurrentTime - num3;
			this.FireProjectile();
		}

		private void FireProjectile()
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.ProjectileEffect);
			effectEvent.MoveSpeed = this.MyInfo.ProjectileMoveSpeed;
			effectEvent.Range = this.MyInfo.ProjectileRange;
			effectEvent.Origin = this.Combat.Transform.position;
			effectEvent.Target = base.Target;
			effectEvent.LifeTime = effectEvent.Range / effectEvent.MoveSpeed;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target);
			effectEvent.Modifiers = this._upgProjectileModifiers;
			this._projectileEffectId = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			base.OnGadgetUsed(this._projectileEffectId);
		}

		protected override void InnerOnDestroyEffect(DestroyEffect evt)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (evt.RemoveData.TargetEventId != this._projectileEffectId)
			{
				return;
			}
			this._projectileEffectId = -1;
			if (evt.RemoveData.TargetId == -1)
			{
				return;
			}
			this.CreateCreepBuff(evt.RemoveData.TargetId, evt.RemoveData.TargetEventId);
		}

		private CreepInfo GetCurrentCreepInfo()
		{
			if (!base.Activated || !this._creepInfos.TryGetValue(this._upgCreepLevel.IntGet(), out this._currentCreepInfo))
			{
				this._currentCreepInfo = this.MyInfo.BaseCreepInfo;
			}
			return this._currentCreepInfo;
		}

		private bool CreepMaxReached
		{
			get
			{
				return this._creepInstances.Count >= ((!base.Activated) ? this.MyInfo.BaseCreepMax : this._upgCreepMax.IntGet());
			}
		}

		public void SpawnCreep(Vector3 position)
		{
			if (this.CreepMaxReached || !this.Combat.IsAlive())
			{
				return;
			}
			position += new Vector3(UnityEngine.Random.value, 0f, UnityEngine.Random.value);
			NavMeshHit navMeshHit;
			if (NavMesh.SamplePosition(position, out navMeshHit, 1000f, 1))
			{
				position = navMeshHit.position;
				CreepInfo currentCreepInfo = this.GetCurrentCreepInfo();
				int creepId = GameHubBehaviour.Hub.Events.Creeps.GetCreepId();
				CreepEvent content = new CreepEvent
				{
					CauserId = this.Combat.Id.ObjId,
					CreepId = creepId,
					CreepInfoId = currentCreepInfo.CreepInfoId,
					Location = position,
					Direction = base.CalcDirection(this.Combat.Transform.position, this.Combat.Transform.position + this.Combat.Transform.forward),
					Reason = SpawnReason.Respawn,
					CreepTeam = this.Combat.Team,
					Level = this._upgCreepLevel.IntGet() + 1
				};
				this._creepInstances.Add(new LadyMuerteCreepBeacon.CreepInstance
				{
					CreepId = creepId,
					EffectId = GameHubBehaviour.Hub.Events.TriggerEvent(content)
				});
				this.UpdateCreepCount();
				return;
			}
			LadyMuerteCreepBeacon.Log.WarnFormat("Failed to spawn creep, could not find a proper position in the NavMesh. Pos={0}", new object[]
			{
				position
			});
		}

		private void OnCreepUnspawn(CreepRemoveEvent data)
		{
			int num = this._creepInstances.FindIndex((LadyMuerteCreepBeacon.CreepInstance c) => c.CreepId == data.CreepId);
			if (num < 0)
			{
				return;
			}
			if (this.CreepMaxReached)
			{
				long num2 = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
				long num3 = num2 + (long)(base.MaxChargeTime * 1000f);
				this._nextCreepRespawnMillis = num3;
				this.ChargeTime = num3;
			}
			this._creepInstances.RemoveAt(num);
			this._creepCache.Remove(data.CreepId);
			this.UpdateCreepCount();
		}

		public override void OnObjectUnspawned(UnspawnEvent evt)
		{
			base.OnObjectUnspawned(evt);
			this._nextCreepRespawnMillis = 0L;
			this.ChargeTime = 0L;
			for (int i = 0; i < this._creepInstances.Count; i++)
			{
				CreepRemoveEvent creepRemoveEvent = new CreepRemoveEvent
				{
					CauserId = -1,
					CreepId = this._creepInstances[i].CreepId,
					Reason = SpawnReason.Disconnect
				};
				Identifiable creepIdentifiable = this.GetCreepIdentifiable(this._creepInstances[i].CreepId);
				if (creepIdentifiable)
				{
					creepRemoveEvent.Location = creepIdentifiable.transform.position;
				}
				GameHubBehaviour.Hub.Events.TriggerEvent(creepRemoveEvent);
			}
			this._creepInstances.Clear();
			this.UpdateCreepCount();
		}

		private Identifiable GetCreepIdentifiable(int creepId)
		{
			Identifiable @object;
			if (!this._creepCache.TryGetValue(creepId, out @object))
			{
				@object = GameHubBehaviour.Hub.ObjectCollection.GetObject(creepId);
				if (@object)
				{
					this._creepCache[creepId] = @object;
				}
			}
			return @object;
		}

		private void CreateCreepBuff(int targetId, int eventId)
		{
			for (int i = 0; i < this._creepInstances.Count; i++)
			{
				Identifiable creepIdentifiable = this.GetCreepIdentifiable(this._creepInstances[i].CreepId);
				if (creepIdentifiable)
				{
					CombatObject component = creepIdentifiable.GetComponent<CombatObject>();
					CreepController component2 = creepIdentifiable.GetComponent<CreepController>();
					if (component)
					{
						component.Controller.AddModifiers(this._upgCreepBuffModifiers, this.Combat, eventId, false);
					}
					if (component2)
					{
						Identifiable @object = GameHubBehaviour.Hub.ObjectCollection.GetObject(targetId);
						CombatObject combatObject = (!@object) ? null : @object.GetComponent<CombatObject>();
						if (combatObject)
						{
							component2.SetCurrentTarget(combatObject, this.MyInfo.CreepBuffLifeTime);
						}
					}
				}
			}
		}

		public override float GetDps()
		{
			CreepBasicAttackInfo creepBasicAttackInfo = (CreepBasicAttackInfo)this.GetCurrentCreepInfo().Attack;
			return base.GetDpsFromModifierData(this._upgProjectileModifiers) + base.GetDpsFromModifierInfoWithCustomCooldown(creepBasicAttackInfo.Modifiers, creepBasicAttackInfo.Cooldown) * (float)this.Combat.GadgetStates.GetGadgetState(base.Slot).Value;
		}

		public override float GetRange()
		{
			return (!(this.MyInfo != null)) ? base.GetRange() : this.MyInfo.ProjectileRange;
		}

		public override float GetRangeSqr()
		{
			return (!(this.MyInfo != null)) ? base.GetRange() : (this.MyInfo.ProjectileRange * this.MyInfo.ProjectileRange);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(LadyMuerteCreepBeacon));

		private ModifierData[] _upgProjectileModifiers;

		private ModifierData[] _upgCreepBuffModifiers;

		private Upgradeable _upgCreepMax;

		private Upgradeable _upgCreepLevel;

		private int _projectileEffectId;

		private CreepInfo _currentCreepInfo;

		private Dictionary<int, CreepInfo> _creepInfos;

		private List<LadyMuerteCreepBeacon.CreepInstance> _creepInstances = new List<LadyMuerteCreepBeacon.CreepInstance>();

		private Dictionary<int, Identifiable> _creepCache = new Dictionary<int, Identifiable>();

		private long _nextCreepRespawnMillis;

		private class CreepInstance
		{
			public int CreepId;

			public int EffectId;
		}
	}
}
