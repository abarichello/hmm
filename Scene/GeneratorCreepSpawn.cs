using System;
using System.Collections.Generic;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;
using UnityEngine.AI;

namespace HeavyMetalMachines.Scene
{
	public class GeneratorCreepSpawn : GameHubBehaviour
	{
		public int TriggerSpawn(GeneratorCreepSpawn.CreepSpawnInfo spawn)
		{
			CreepInfo creepInfo = (!this.IsMega || !spawn.Mega) ? spawn.Info : spawn.Mega;
			CreepEvent creepEvent = new CreepEvent();
			creepEvent.CauserId = this.Parent.ObjId;
			creepEvent.CreepId = GameHubBehaviour.Hub.Events.Creeps.GetCreepId(spawn.Amount);
			creepEvent.CreepInfoId = creepInfo.CreepInfoId;
			creepEvent.Location = this.CreepSpawnPoint.position;
			creepEvent.Direction = this.CreepSpawnPoint.forward;
			creepEvent.Reason = SpawnReason.Respawn;
			creepEvent.Amount = spawn.Amount;
			creepEvent.CreepTeam = this.Team;
			creepEvent.Level = 1 + Mathf.FloorToInt(Mathf.Max(0f, (float)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() * 0.001f - (float)GameHubBehaviour.Hub.MatchMan.WarmupSeconds) / 60f);
			GameHubBehaviour.Hub.Events.TriggerEvent(creepEvent);
			if (this.OnNextSpawn != null)
			{
				this.OnNextSpawn();
				this.OnNextSpawn = null;
			}
			return creepEvent.CreepId;
		}

		private void Start()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				this.PrecacheCreeps();
				base.enabled = false;
				return;
			}
			if (!this.Parent)
			{
				this.Parent = base.Id;
			}
			this.CalcPath();
			this.PrecacheCreeps();
			this.InitializeCreeps();
			GameHubBehaviour.Hub.Events.Creeps.ListenToCreepUnspawn += this.ListenToCreepUnspawn;
		}

		private void OnDestroy()
		{
			GameHubBehaviour.Hub.Events.Creeps.ListenToCreepUnspawn -= this.ListenToCreepUnspawn;
			GeneratorCreepSpawn.CachedCreeps.Clear();
		}

		private void InitializeCreeps()
		{
			for (int i = 0; i < this.SpawnInfos.Length; i++)
			{
				GeneratorCreepSpawn.CreepSpawnInfo creepSpawnInfo = this.SpawnInfos[i];
				if (!creepSpawnInfo.Updater.IsInitialized())
				{
					if (creepSpawnInfo.Delay > 0)
					{
						creepSpawnInfo.Updater.PeriodSeconds = creepSpawnInfo.Delay;
						creepSpawnInfo.Updater.ShouldHalt();
						creepSpawnInfo.InDelay = true;
					}
					else
					{
						creepSpawnInfo.Updater.PeriodSeconds = creepSpawnInfo.Cycle;
						creepSpawnInfo.InDelay = false;
					}
				}
			}
		}

		private void PrecacheCreeps()
		{
			int num = 90;
			for (int i = 0; i < this.SpawnInfos.Length; i++)
			{
				GeneratorCreepSpawn.CreepSpawnInfo creepSpawnInfo = this.SpawnInfos[i];
				if (creepSpawnInfo != null)
				{
					int num2 = num / creepSpawnInfo.Cycle;
					if (num2 < 1)
					{
						num2 = 1;
					}
					this.CacheCreeps(creepSpawnInfo.Info, creepSpawnInfo.Amount * num2, false);
					if (creepSpawnInfo.Mega != null)
					{
						GameHubBehaviour.Hub.Events.Creeps.Factory.CacheCreeps(creepSpawnInfo.Mega, creepSpawnInfo.Amount * num2);
					}
				}
			}
		}

		public void CacheCreeps(CreepInfo info, int amount, bool onlySpawnAfterDeath = false)
		{
			if (!onlySpawnAfterDeath)
			{
				amount *= 2;
				int num;
				GeneratorCreepSpawn.CachedCreeps.TryGetValue(info, out num);
				if (num >= GeneratorCreepSpawn.MaxCachedCreepsPerType)
				{
					return;
				}
				GeneratorCreepSpawn.CachedCreeps[info] = num + amount;
			}
			GameHubBehaviour.Hub.Events.Creeps.Factory.CacheCreeps(info, amount);
		}

		private void CalcPath()
		{
			this._navPath = new NavMeshPath[this.Path.Length];
			Vector3 sourcePosition = this.CreepSpawnPoint.position;
			for (int i = 0; i < this.Path.Length; i++)
			{
				Vector3 vector = this.Path[i];
				this._navPath[i] = new NavMeshPath();
				NavMesh.CalculatePath(sourcePosition, vector, -1, this._navPath[i]);
				sourcePosition = vector;
			}
		}

		public NavMeshPath[] GetPath()
		{
			return this._navPath;
		}

		private void FixedUpdate()
		{
			if (!GameHubBehaviour.Hub.MatchMan.WarmupDone)
			{
				return;
			}
			this.UpdateSpawnInfos();
		}

		public void UpdateSpawnInfos()
		{
			if (!this.IsMega && this.Mirror && !this.Mirror.IsAlive())
			{
				this.IsMega = true;
			}
			for (int i = 0; i < this.SpawnInfos.Length; i++)
			{
				GeneratorCreepSpawn.CreepSpawnInfo creepSpawnInfo = this.SpawnInfos[i];
				if (!creepSpawnInfo.Updater.ShouldHalt())
				{
					if (creepSpawnInfo.InDelay)
					{
						creepSpawnInfo.InDelay = false;
						creepSpawnInfo.Updater.PeriodSeconds = creepSpawnInfo.Cycle;
						creepSpawnInfo.Updater.Reset();
						creepSpawnInfo.Updater.ShouldHalt();
					}
					CreepRespawnKind respawnKind = creepSpawnInfo.Info.RespawnKind;
					if (respawnKind != CreepRespawnKind.Timer)
					{
						if (respawnKind == CreepRespawnKind.AfterDeath)
						{
							if (!this._respawnAfterDeathCreeps.ContainsValue(i))
							{
								int num = this.TriggerSpawn(creepSpawnInfo);
								if (num >= 0)
								{
									this._respawnAfterDeathCreeps[num] = i;
								}
							}
						}
					}
					else
					{
						this.TriggerSpawn(creepSpawnInfo);
					}
				}
			}
		}

		private void ListenToCreepUnspawn(CreepRemoveEvent data)
		{
			if (!this._respawnAfterDeathCreeps.ContainsKey(data.CreepId))
			{
				return;
			}
			int num = this._respawnAfterDeathCreeps[data.CreepId];
			this.SpawnInfos[num].Updater.Reset();
			this._respawnAfterDeathCreeps.Remove(data.CreepId);
		}

		public void DestroyAllCreeps()
		{
			foreach (KeyValuePair<int, int> keyValuePair in this._respawnAfterDeathCreeps)
			{
				CreepRemoveEvent content = new CreepRemoveEvent
				{
					Location = base.transform.position,
					CreepId = keyValuePair.Key,
					CauserId = -1,
					Reason = SpawnReason.ScoreBoard
				};
				GameHubBehaviour.Hub.Events.TriggerEvent(content);
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(GeneratorCreepSpawn));

		private readonly Dictionary<int, int> _respawnAfterDeathCreeps = new Dictionary<int, int>();

		public Identifiable Parent;

		public TeamKind Team;

		public GeneratorCreepSpawn.CreepSpawnInfo[] SpawnInfos;

		public Transform CreepSpawnPoint;

		public Vector3[] Path;

		public BotPatrolController PatrolController;

		public BotAIPath AIPath;

		private NavMeshPath[] _navPath;

		public bool IsMega;

		public CombatObject Mirror;

		public Action OnNextSpawn;

		public static int MaxCachedCreepsPerType = 30;

		public static Dictionary<CreepInfo, int> CachedCreeps = new Dictionary<CreepInfo, int>();

		[Serializable]
		public class CreepSpawnInfo
		{
			public bool InDelay { get; set; }

			public CreepInfo Info;

			public CreepInfo Mega;

			public int Amount;

			public int Cycle;

			public int Delay;

			public TimedUpdater Updater;
		}
	}
}
