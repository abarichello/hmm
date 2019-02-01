using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;
using Pocketverse.MuralContext;

namespace HeavyMetalMachines.Combat
{
	public class CreepSpawnManager : GameHubBehaviour, ICleanupListener
	{
		public Dictionary<int, SpawnController> AllCreeps
		{
			get
			{
				return this._allCreeps;
			}
		}

		public int GetCreepId()
		{
			return ObjectId.New(ContentKind.Creeps.Byte(), ++this._creepId);
		}

		public int GetCreepId(int amount)
		{
			int result = ObjectId.New(ContentKind.Creeps.Byte(), ++this._creepId);
			this._creepId += amount - 1;
			return result;
		}

		public CombatObject GetCreep(int objectId)
		{
			SpawnController spawnController;
			if (this._allCreeps.TryGetValue(objectId, out spawnController))
			{
				return spawnController.Combat;
			}
			return null;
		}

		public List<CreepController> GetBlueCreepList()
		{
			return this._blueCreepsList;
		}

		public List<CreepController> GetRedCreepList()
		{
			return this._redCreepsList;
		}

		public void Trigger(IEventContent evt, int eventId)
		{
			CreepEvent creepEvent = evt as CreepEvent;
			if (creepEvent != null)
			{
				if (creepEvent.Amount > 1)
				{
					this.m_cSpawnData.Enqueue(new CreepSpawnManager.CSpawnData(creepEvent, eventId));
				}
				else
				{
					this.Spawn(creepEvent, creepEvent.CreepId, eventId);
				}
			}
			else
			{
				this.Unspawn((CreepRemoveEvent)evt);
			}
		}

		private void Update()
		{
			if (this.m_oCurrentSpawnData == null || this.m_oCurrentSpawnData.CurrentCount <= 0)
			{
				if (this.m_cSpawnData.Count == 0)
				{
					this.m_oCurrentSpawnData = null;
					return;
				}
				this.m_oCurrentSpawnData = this.m_cSpawnData.Dequeue();
			}
			if (!GameHubBehaviour.Hub.Match.State.IsGame())
			{
				this.m_oCurrentSpawnData = null;
				this.m_cSpawnData.Clear();
				return;
			}
			this.Spawn(this.m_oCurrentSpawnData.m_oCreepEvent, this.m_oCurrentSpawnData.CurrentCreepId, this.m_oCurrentSpawnData.m_nEventId);
			this.m_oCurrentSpawnData.CurrentCount--;
			this.m_oCurrentSpawnData.CurrentCreepId = this.m_oCurrentSpawnData.CurrentCreepId.NextInstanceId();
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CreepSpawnManager.CreepSpawnListener ListenToCreepSpawn;

		private void Spawn(CreepEvent data, int creepId, int eventId)
		{
			if (GameHubBehaviour.Hub.Match.State == MatchData.MatchState.MatchOverBluWins || GameHubBehaviour.Hub.Match.State == MatchData.MatchState.MatchOverRedWins)
			{
				return;
			}
			eventId = GameHubBehaviour.Hub.Events.CreateAndBufferEvent(data.SingleClone());
			Identifiable identifiable = this.Factory.CreateCreep(data, creepId);
			SpawnController bitComponent = identifiable.GetBitComponent<SpawnController>();
			bitComponent.StartPosition = (bitComponent.SpawnPosition = base.transform);
			bitComponent.Spawn(data.Location, data.Direction, eventId, data.Reason);
			this._allCreeps[creepId] = bitComponent;
			TeamKind creepTeam = data.CreepTeam;
			if (creepTeam != TeamKind.Blue)
			{
				if (creepTeam == TeamKind.Red)
				{
					if (!this._redCreepsList.Contains(bitComponent.Combat.Creep))
					{
						this._redCreepsList.Add(bitComponent.Combat.Creep);
					}
				}
			}
			else if (!this._blueCreepsList.Contains(bitComponent.Combat.Creep))
			{
				this._blueCreepsList.Add(bitComponent.Combat.Creep);
			}
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this._creepSpawnEvents[creepId] = eventId;
			}
			if (this.ListenToCreepSpawn != null)
			{
				this.ListenToCreepSpawn(bitComponent.Combat.Creep);
			}
		}

		private void Unspawn(CreepRemoveEvent data)
		{
			SpawnController spawnController;
			if (!this._allCreeps.TryGetValue(data.CreepId, out spawnController))
			{
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsServer() && this.ListenToCreepUnspawn != null)
			{
				this.ListenToCreepUnspawn(data);
			}
			if (GameHubBehaviour.Hub.Net.IsServer() && data.Reason == SpawnReason.Death)
			{
				GameHubBehaviour.Hub.ScrapBank.CreepKilled(data.CreepId, data.CauserId);
				CreepSpawnManager.Log.InfoFormat("CreepKilled causer={0} MatchTime={1}", new object[]
				{
					data.CauserId,
					(float)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() / 1000f
				});
			}
			spawnController.Unspawn(data.Location, data.Reason, data.CauserId);
			this._allCreeps.Remove(data.CreepId);
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				int eventId = this._creepSpawnEvents[data.CreepId];
				GameHubBehaviour.Hub.Events.ForgetEvent(eventId);
				this._creepSpawnEvents.Remove(data.CreepId);
				if (data.Reason == SpawnReason.Death && data.CauserId.GetTypeId() == ContentKind.PlayerCar.Byte())
				{
					BombMatchBI.CreepKilled(data.CauserId);
				}
			}
			CreepController creepController = this._blueCreepsList.Find((CreepController c) => c.Id.ObjId == data.CreepId);
			if (creepController != null)
			{
				this._blueCreepsList.Remove(creepController);
			}
			else
			{
				creepController = this._redCreepsList.Find((CreepController c) => c.Id.ObjId == data.CreepId);
				if (creepController != null)
				{
					this._redCreepsList.Remove(creepController);
				}
			}
			this.Factory.DestroyCreep(spawnController);
		}

		public void UnspawnAllCreeps()
		{
			foreach (KeyValuePair<int, SpawnController> keyValuePair in this._allCreeps)
			{
				CreepRemoveEvent content = new CreepRemoveEvent
				{
					Location = base.transform.position,
					CreepId = keyValuePair.Key,
					CauserId = -1,
					Reason = SpawnReason.None
				};
				GameHubBehaviour.Hub.Events.TriggerEvent(content);
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CreepSpawnManager.CreepUnspawnListener ListenToCreepUnspawn;

		public void OnCleanup(CleanupMessage msg)
		{
			this._allCreeps.Clear();
			this._creepSpawnEvents.Clear();
			this._blueCreepsList.Clear();
			this._redCreepsList.Clear();
			this._creepId = 0;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CreepSpawnManager));

		public CreepFactory Factory;

		private readonly Dictionary<int, SpawnController> _allCreeps = new Dictionary<int, SpawnController>();

		private readonly List<CreepController> _blueCreepsList = new List<CreepController>();

		private readonly List<CreepController> _redCreepsList = new List<CreepController>();

		private readonly Dictionary<int, int> _creepSpawnEvents = new Dictionary<int, int>();

		private int _creepId;

		private Queue<CreepSpawnManager.CSpawnData> m_cSpawnData = new Queue<CreepSpawnManager.CSpawnData>(10);

		private CreepSpawnManager.CSpawnData m_oCurrentSpawnData;

		private class CSpawnData
		{
			public CSpawnData(CreepEvent oCreepEvent, int nEventId)
			{
				this.m_oCreepEvent = oCreepEvent;
				this.m_nEventId = nEventId;
				this.CurrentCreepId = oCreepEvent.CreepId;
				this.CurrentCount = oCreepEvent.Amount;
			}

			public CreepEvent m_oCreepEvent;

			public int m_nEventId;

			public int CurrentCreepId;

			public int CurrentCount;
		}

		public delegate void CreepSpawnListener(CreepController creepController);

		public delegate void CreepUnspawnListener(CreepRemoveEvent data);
	}
}
