using System;
using System.Collections;
using System.Collections.Generic;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Scene
{
	public class ActivatedCreepSpawn : GameHubBehaviour, IActivatable
	{
		public void Activate(bool enable, int causer)
		{
			if (!enable)
			{
				return;
			}
			if (causer != -1)
			{
				Identifiable @object = GameHubBehaviour.Hub.ObjectCollection.GetObject(causer);
				CombatObject bitComponent;
				if (@object != null && (bitComponent = @object.GetBitComponent<CombatObject>()) != null && bitComponent.Team != this.Team && this.Team != TeamKind.Neutral)
				{
					return;
				}
			}
			int charges = 1;
			if (this.SpawnDelaySeconds > 0f)
			{
				base.StartCoroutine(this.DelayedTriggerSpawn(charges));
			}
			else if (this.OutOfSynchSpawn)
			{
				this.TriggerSpawn(charges);
			}
			else
			{
				this.Generator.OnNextSpawn = delegate()
				{
					this.TriggerSpawn(charges);
				};
			}
		}

		private IEnumerator DelayedTriggerSpawn(int charges)
		{
			yield return this.waitForSpawnDelay;
			if (this.OutOfSynchSpawn)
			{
				this.TriggerSpawn(charges);
			}
			else
			{
				this.Generator.OnNextSpawn = delegate()
				{
					this.TriggerSpawn(charges);
				};
			}
			yield break;
		}

		private void TriggerSpawn(int charges)
		{
			for (int i = 0; i < this.SpawnInfos.Length; i++)
			{
				ActivatedCreepSpawn.CreepSpawnInfo creepSpawnInfo = this.SpawnInfos[i];
				if (creepSpawnInfo.AnnouncerEventKind != AnnouncerLog.AnnouncerEventKinds.None)
				{
					this.ThrowAnnounceEvent(creepSpawnInfo.AnnouncerEventKind);
				}
				int num = creepSpawnInfo.Amount * charges;
				if (this.OnlySpawnAfterDeath)
				{
					num -= creepSpawnInfo.SpawnedCreeps.Count;
				}
				for (int j = 0; j < num; j++)
				{
					Transform transform = (this.CreepSpawnPoints.Length != 0) ? ((this.CreepSpawnPoints.Length <= j) ? this.CreepSpawnPoints[0] : this.CreepSpawnPoints[j]) : this.Generator.CreepSpawnPoint;
					CreepEvent creepEvent = new CreepEvent
					{
						CauserId = this.Generator.Id.ObjId,
						CreepId = GameHubBehaviour.Hub.Events.Creeps.GetCreepId(),
						CreepInfoId = creepSpawnInfo.Info.CreepInfoId,
						Location = transform.position,
						Direction = transform.forward,
						Reason = SpawnReason.Respawn,
						BotAggroMaxDistance = creepSpawnInfo.BotAggroMaxDistance,
						CreepTeam = this.Team,
						Level = 1 + Mathf.FloorToInt(Mathf.Max(0f, (float)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() * 0.001f - (float)GameHubBehaviour.Hub.MatchMan.WarmupSeconds) / 60f)
					};
					GameHubBehaviour.Hub.Events.TriggerEvent(creepEvent);
					creepSpawnInfo.SpawnedCreeps.Add(creepEvent.CreepId);
				}
			}
		}

		private void ThrowAnnounceEvent(AnnouncerLog.AnnouncerEventKinds logKind)
		{
			if (logKind == AnnouncerLog.AnnouncerEventKinds.None)
			{
				return;
			}
			AnnouncerEvent content = new AnnouncerEvent
			{
				AnnouncerEventKind = logKind
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
		}

		private void OnEnable()
		{
			GameHubBehaviour.Hub.Events.Creeps.ListenToCreepUnspawn += this.ListenToCreepUnspawn;
			this.CacheCreeps();
		}

		private void OnDisable()
		{
			GameHubBehaviour.Hub.Events.Creeps.ListenToCreepUnspawn -= this.ListenToCreepUnspawn;
		}

		private void CacheCreeps()
		{
			if (GameHubBehaviour.Hub.State.Current.StateKind == GameState.GameStateKind.Game)
			{
				return;
			}
			for (int i = 0; i < this.SpawnInfos.Length; i++)
			{
				ActivatedCreepSpawn.CreepSpawnInfo creepSpawnInfo = this.SpawnInfos[i];
				if (creepSpawnInfo != null && !(creepSpawnInfo.Info == null))
				{
					this.Generator.CacheCreeps(creepSpawnInfo.Info, creepSpawnInfo.Amount, this.OnlySpawnAfterDeath);
				}
			}
		}

		private void ListenToCreepUnspawn(CreepRemoveEvent data)
		{
			for (int i = 0; i < this.SpawnInfos.Length; i++)
			{
				ActivatedCreepSpawn.CreepSpawnInfo creepSpawnInfo = this.SpawnInfos[i];
				if (creepSpawnInfo.SpawnedCreeps.Contains(data.CreepId))
				{
					creepSpawnInfo.SpawnedCreeps.Remove(data.CreepId);
					break;
				}
			}
		}

		private void Awake()
		{
			if (!this.ExitPoint)
			{
				this.ExitPoint = base.transform;
			}
			this.waitForSpawnDelay = new WaitForSeconds(this.SpawnDelaySeconds);
		}

		public void DestroyAllCreeps()
		{
			for (int i = 0; i < this.SpawnInfos.Length; i++)
			{
				ActivatedCreepSpawn.CreepSpawnInfo creepSpawnInfo = this.SpawnInfos[i];
				for (int j = 0; j < creepSpawnInfo.SpawnedCreeps.Count; j++)
				{
					int num = creepSpawnInfo.SpawnedCreeps[j];
					Identifiable @object = GameHubBehaviour.Hub.ObjectCollection.GetObject(num);
					BotAIController component = @object.GetComponent<BotAIController>();
					int currentCreepId = num;
					component.OnArrival += delegate()
					{
						CreepRemoveEvent content = new CreepRemoveEvent
						{
							Location = this.transform.position,
							CreepId = currentCreepId,
							CauserId = -1,
							Reason = SpawnReason.ScoreBoard
						};
						GameHubBehaviour.Hub.Events.TriggerEvent(content);
					};
				}
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ActivatedCreepSpawn));

		public float SpawnDelaySeconds;

		public TeamKind Team;

		public GeneratorCreepSpawn Generator;

		public bool OutOfSynchSpawn;

		public Transform ExitPoint;

		public bool OnlySpawnAfterDeath;

		public Transform[] CreepSpawnPoints;

		public ActivatedCreepSpawn.CreepSpawnInfo[] SpawnInfos;

		private WaitForSeconds waitForSpawnDelay;

		[Serializable]
		public class CreepSpawnInfo
		{
			public CreepInfo Info;

			public int Amount;

			public float BotAggroMaxDistance;

			public AnnouncerLog.AnnouncerEventKinds AnnouncerEventKind;

			[NonSerialized]
			public readonly List<int> SpawnedCreeps = new List<int>();
		}
	}
}
