using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class BaseSpawnManager : GameHubBehaviour, ICleanupListener
	{
		public LevelSpawn Spawn
		{
			get
			{
				LevelSpawn result;
				if ((result = this._spawn) == null)
				{
					result = (this._spawn = (LevelSpawn)UnityEngine.Object.FindObjectOfType(typeof(LevelSpawn)));
				}
				return result;
			}
			set
			{
				this._spawn = value;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BaseSpawnManager.PlayerSpawnListener ListenToPreObjectSpawn;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BaseSpawnManager.PlayerSpawnListener ListenToObjectRespawning;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BaseSpawnManager.PlayerSpawnListener ListenToObjectSpawn;

		public virtual bool CarCreationFinished
		{
			get
			{
				return this.FirstSpawnFinished;
			}
			set
			{
				this.FirstSpawnFinished = value;
			}
		}

		protected virtual List<PlayerData> ObjectList
		{
			get
			{
				return null;
			}
		}

		protected virtual PlayerEvent CreateSpawnData()
		{
			return new PlayerEvent();
		}

		protected virtual PlayerCarFactory GetFactory()
		{
			return null;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action ListenToAllPlayersSpawned;

		protected virtual void CallListenToAllPlayersSpawned()
		{
			if (this.ListenToAllPlayersSpawned != null)
			{
				this.ListenToAllPlayersSpawned();
			}
		}

		protected virtual void OnPlayerOwnerCreated(PlayerEvent player)
		{
		}

		private void Awake()
		{
			this._update = new TimedUpdater(250, true, false);
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChanged;
		}

		private void Update()
		{
			if (GameHubBehaviour.Hub.Net.IsClient() || this._update.ShouldHalt() || GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				return;
			}
			for (int i = 0; i < this.Players.Count; i++)
			{
				this.CheckPlayer(this.Players[i]);
			}
		}

		protected virtual void OnDestroy()
		{
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChanged;
			this.ListenToAllPlayersSpawned = null;
			this.ListenToObjectSpawn = null;
			this.ListenToObjectUnspawn = null;
			this.ListenToAllPlayersSpawned = null;
			this.ListenToObjectDeath = null;
		}

		private void OnPhaseChanged(BombScoreBoard.State state)
		{
			if (state != BombScoreBoard.State.Shop)
			{
				return;
			}
			for (int i = 0; i < this.Players.Count; i++)
			{
				SpawnController spawnController = this.Players[i];
				if (!spawnController.Combat.IsAlive())
				{
					Transform spawn = spawnController.GetSpawn();
					this.SpawnPlayer(spawnController, spawn.position, spawn.forward, SpawnReason.ScoreBoard);
				}
			}
		}

		public void SpawnAllObjects()
		{
			for (int i = 0; i < this.ObjectList.Count; i++)
			{
				this.CreatePlayer(this.ObjectList[i]);
			}
			if (!GameHubBehaviour.Hub.Match.LevelIsTutorial() && !this.CarCreationFinished && this.ObjectList.Count == 0)
			{
				this.CarCreationFinished = true;
				if (this.ListenToAllPlayersSpawned != null)
				{
					this.ListenToAllPlayersSpawned();
				}
			}
		}

		protected virtual void CreatePlayer(PlayerData player)
		{
			int objId = player.PlayerCarId;
			BaseSpawnManager.Log.InfoFormat("Creating character={1} for={0} ObjectId={2}", new object[]
			{
				player.PlayerAddress,
				player.Character.Character,
				objId
			});
			Transform transform = (!this.Spawn) ? base.transform : this.Spawn.GetStart(player);
			PlayerEvent spawnData = this.CreateSpawnData();
			spawnData.TargetId = objId;
			spawnData.CauserId = -1;
			spawnData.Direction = transform.forward;
			spawnData.Location = transform.position;
			spawnData.SourceEventId = -1;
			spawnData.PlayerAddress = player.PlayerAddress;
			spawnData.EventKind = PlayerEvent.Kind.Create;
			IFuture<Identifiable> fut = new Future<Identifiable>();
			this.GetFactory().OrderObject(objId, fut);
			Vector3 spawnPos = transform.position;
			Quaternion spawnRot = transform.rotation;
			this.BuildQueue.Add(objId, fut);
			fut.WhenDone(delegate(IFuture future)
			{
				player.CharacterInstance = fut.Result;
				player.CharacterInstance.transform.position = spawnPos;
				player.CharacterInstance.transform.rotation = spawnRot;
				SpawnController component = player.CharacterInstance.GetComponent<SpawnController>();
				if (component)
				{
					component.StartPosition = ((!this.Spawn) ? this.transform : this.Spawn.GetStart(player));
					component.SpawnPosition = ((!this.Spawn) ? this.transform : this.Spawn.GetSpawn(player));
					component.Player = player;
					component.RespawnTimeMillis = this.RespawnTimeSeconds * 1000;
					this.Players.Add(component);
				}
				this.CreatedPlayers++;
				if (!this.CarCreationFinished && this.CreatedPlayers >= this.ObjectList.Count)
				{
					this.CarCreationFinished = true;
					if (this.ListenToAllPlayersSpawned != null)
					{
						this.ListenToAllPlayersSpawned();
					}
				}
				if (GameHubBehaviour.Hub.Net.IsClient() || GameHubBehaviour.Hub.Net.IsTest())
				{
					Mural.PostAll(new PlayerBuildComplete(objId, fut.Result), typeof(PlayerBuildComplete.IPlayerBuildCompleteListener));
					Mural.PostDeep(new MyPlayerBuildComplete(fut.Result), fut.Result);
					this.BuildQueue.Remove(objId);
					this.Respawn(spawnData, -1);
					if (player.CharacterInstance.Id.IsOwner)
					{
						this.OnPlayerOwnerCreated(spawnData);
					}
					if (!GameHubBehaviour.Hub.Net.IsTest())
					{
						return;
					}
				}
				PlayerController component2 = player.CharacterInstance.GetComponent<PlayerController>();
				if (component2)
				{
					GameHubBehaviour.Hub.Input.Register(component2, player.PlayerAddress);
				}
				PlayerStats component3 = player.CharacterInstance.GetComponent<PlayerStats>();
				if (component3)
				{
					GameHubBehaviour.Hub.ScrapBank.RegisterPlayer(objId, component3);
				}
				this.BuildQueue.Remove(objId);
				this.Respawn(spawnData, -1);
			});
		}

		protected void PreSpawnObject(PlayerData player)
		{
			PlayerEvent playerEvent = this.CreateSpawnData();
			playerEvent.TargetId = player.PlayerCarId;
			playerEvent.CauserId = -1;
			playerEvent.SourceEventId = -1;
			playerEvent.EventKind = PlayerEvent.Kind.PreRespawn;
			playerEvent.PlayerAddress = player.PlayerAddress;
			int preSpawnEventId = GameHubBehaviour.Hub.Events.TriggerEvent(playerEvent);
			BaseSpawnManager.PlayerRespawnInfos value;
			this.PlayerRespawnInfosDic.TryGetValue(playerEvent.TargetId, out value);
			value.PreSpawnEventId = preSpawnEventId;
			this.PlayerRespawnInfosDic[playerEvent.TargetId] = value;
		}

		public void SetRespawningData(int playerCarId, Vector3 position, Vector3 forward, SpawnReason reason)
		{
			BaseSpawnManager.PlayerRespawnInfos value;
			this.PlayerRespawnInfosDic.TryGetValue(playerCarId, out value);
			value.RespawningPositionSet = true;
			value.Position = position;
			value.Forward = forward;
			this.PlayerRespawnInfosDic[playerCarId] = value;
		}

		private void RespawningPlayer(PlayerData player, Vector3 position, Vector3 forward, SpawnReason reason)
		{
			PlayerEvent playerEvent = this.CreateSpawnData();
			playerEvent.TargetId = player.PlayerCarId;
			playerEvent.CauserId = -1;
			playerEvent.Direction = forward;
			playerEvent.Location = position;
			playerEvent.SourceEventId = -1;
			playerEvent.Reason = reason;
			playerEvent.EventKind = PlayerEvent.Kind.Respawning;
			playerEvent.PlayerAddress = player.PlayerAddress;
			int respawningEventId = GameHubBehaviour.Hub.Events.TriggerEvent(playerEvent);
			BaseSpawnManager.PlayerRespawnInfos value;
			this.PlayerRespawnInfosDic.TryGetValue(playerEvent.TargetId, out value);
			value.RespawningEventId = respawningEventId;
			value.RespawningPositionSet = false;
			this.PlayerRespawnInfosDic[playerEvent.TargetId] = value;
		}

		public void SetSpawnData(PlayerData player, Vector3 position, Vector3 forward, SpawnReason reason)
		{
			BaseSpawnManager.PlayerRespawnInfos value;
			if (!this.PlayerRespawnInfosDic.TryGetValue(player.PlayerCarId, out value))
			{
				return;
			}
			value.SpawnPositionSet = true;
			value.Position = position;
			value.Forward = forward;
			this.PlayerRespawnInfosDic[player.PlayerCarId] = value;
		}

		public void SpawnPlayer(SpawnController controller, Vector3 position, Vector3 forward, SpawnReason reason)
		{
			this.ForgetEvents(controller.Player.PlayerCarId);
			PlayerEvent playerEvent = this.CreateSpawnData();
			playerEvent.TargetId = controller.Player.PlayerCarId;
			playerEvent.CauserId = -1;
			playerEvent.Direction = forward;
			playerEvent.Location = position;
			playerEvent.SourceEventId = -1;
			playerEvent.Reason = reason;
			playerEvent.EventKind = PlayerEvent.Kind.Respawn;
			playerEvent.PlayerAddress = controller.Player.PlayerAddress;
			GameHubBehaviour.Hub.Events.TriggerEvent(playerEvent);
		}

		protected virtual void PreRespawn(PlayerEvent data, int eventId)
		{
			BaseSpawnManager.Log.InfoFormat("PreRespawn Event received player={0} eventId={1}", new object[]
			{
				data.TargetId,
				eventId
			});
			for (int i = 0; i < this.Players.Count; i++)
			{
				if (this.Players[i].Player.PlayerCarId == data.TargetId)
				{
					this.Players[i].PreSpawn();
					break;
				}
			}
			if (this.ListenToPreObjectSpawn != null)
			{
				this.ListenToPreObjectSpawn(data);
			}
		}

		protected virtual void Respawning(PlayerEvent data, int eventId)
		{
			BaseSpawnManager.Log.InfoFormat("Respawning player={0} eventId={1}", new object[]
			{
				data.TargetId,
				eventId
			});
			for (int i = 0; i < this.Players.Count; i++)
			{
				if (this.Players[i].Player.PlayerCarId == data.TargetId)
				{
					this.Players[i].Respawning();
					break;
				}
			}
			if (this.ListenToObjectRespawning != null)
			{
				this.ListenToObjectRespawning(data);
			}
		}

		protected void Respawn(PlayerEvent data, int eventId)
		{
			BaseSpawnManager.Log.InfoFormat("Spawned player={0} eventId={1} Location={2} Direction={3} Reason={4}", new object[]
			{
				data.TargetId,
				eventId,
				data.Location,
				data.Direction,
				data.Reason
			});
			bool flag = false;
			for (int i = 0; i < this.Players.Count; i++)
			{
				if (this.Players[i].Player.PlayerCarId == data.TargetId)
				{
					flag = true;
					this.Players[i].Spawn(data.Location, data.Direction, -1, data.Reason);
					break;
				}
			}
			if (!flag)
			{
				IFuture<Identifiable> future;
				if (this.BuildQueue.TryGetValue(data.TargetId, out future))
				{
					future.WhenDone(delegate(IFuture x)
					{
						this.Respawn(data, eventId);
					});
				}
				return;
			}
			if (this.ListenToObjectSpawn != null)
			{
				this.ListenToObjectSpawn(data);
			}
		}

		private void Unspawn(PlayerEvent data, int eventId)
		{
			BaseSpawnManager.Log.InfoFormat("Unspawning player={0} eventId={1} Position={2}", new object[]
			{
				data.TargetId,
				eventId,
				data.Location
			});
			for (int i = 0; i < this.Players.Count; i++)
			{
				if (this.Players[i].Player.PlayerCarId == data.TargetId)
				{
					this.Players[i].Unspawn(data.Location, data.Reason, data.CauserId);
					break;
				}
			}
			BaseSpawnManager.PlayerRespawnInfos value;
			this.PlayerRespawnInfosDic.TryGetValue(data.TargetId, out value);
			value.UnspawnEventId = eventId;
			this.PlayerRespawnInfosDic[data.TargetId] = value;
			if (this.ListenToObjectUnspawn == null)
			{
				return;
			}
			this.ListenToObjectUnspawn(data);
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BaseSpawnManager.PlayerUnspawnListener ListenToObjectUnspawn;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BaseSpawnManager.PlayerUnspawnListener ListenToObjectDeath;

		private void CheckPlayer(SpawnController controller)
		{
			if (GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreBoard.State.PreReplay || GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreBoard.State.Replay)
			{
				return;
			}
			switch (controller.State)
			{
			case SpawnController.StateType.Unspawned:
				if (!controller.ShouldFinishDeathTime())
				{
					return;
				}
				this.DeathTimeFinished(controller);
				break;
			case SpawnController.StateType.PreSpawned:
			{
				BaseSpawnManager.PlayerRespawnInfos playerRespawnInfos;
				this.PlayerRespawnInfosDic.TryGetValue(controller.Player.PlayerCarId, out playerRespawnInfos);
				if (!playerRespawnInfos.RespawningPositionSet)
				{
					if (!controller.Player.IsBotControlled)
					{
						return;
					}
					this.ForceGadgetActivation(controller, GameHubBehaviour.Hub.BombManager.BombMovement.transform.position, GameHubBehaviour.Hub.BombManager.BombMovement.transform.forward);
					return;
				}
				else
				{
					this.RespawningPlayer(controller.Player, playerRespawnInfos.Position, playerRespawnInfos.Forward, SpawnReason.Respawn);
				}
				break;
			}
			case SpawnController.StateType.Respawning:
			{
				BaseSpawnManager.PlayerRespawnInfos playerRespawnInfos;
				this.PlayerRespawnInfosDic.TryGetValue(controller.Player.PlayerCarId, out playerRespawnInfos);
				if (!playerRespawnInfos.SpawnPositionSet)
				{
					return;
				}
				this.SpawnPlayer(controller, playerRespawnInfos.Position, playerRespawnInfos.Forward, SpawnReason.Respawn);
				break;
			}
			}
		}

		protected virtual void DeathTimeFinished(SpawnController controller)
		{
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState != BombScoreBoard.State.BombDelivery)
			{
				Transform spawn = controller.GetSpawn();
				this.ForceObjectSpawn(controller, spawn.position, spawn.forward);
			}
			else
			{
				this.PreSpawnObject(controller.Player);
			}
		}

		protected void ForceGadgetActivation(SpawnController controller, Vector3 position, Vector3 forward)
		{
			controller.Combat.RespawnGadget.Pressed = true;
			controller.Combat.RespawnGadget.Dir = forward;
			controller.Combat.RespawnGadget.Target = position;
		}

		public virtual void ForceObjectSpawn(SpawnController controller, Vector3 position, Vector3 forward)
		{
			controller.ForceFinishDeathTime();
			BaseSpawnManager.PlayerRespawnInfos value;
			this.PlayerRespawnInfosDic.TryGetValue(controller.Player.PlayerCarId, out value);
			value.RespawningPositionSet = true;
			value.SpawnPositionSet = true;
			value.Position = position;
			value.Forward = forward;
			this.PlayerRespawnInfosDic[controller.Player.PlayerCarId] = value;
		}

		public void ForgetEvents(int playerCarId)
		{
			BaseSpawnManager.PlayerRespawnInfos playerRespawnInfos;
			this.PlayerRespawnInfosDic.TryGetValue(playerCarId, out playerRespawnInfos);
			BaseSpawnManager.Log.InfoFormat("Forget event for playerCarID {0}, DeathEvent {1}, UnspawEvent {2}, PreSpawnEventId {3}, RespawningEventId {4}", new object[]
			{
				playerCarId,
				playerRespawnInfos.DeathEventId,
				playerRespawnInfos.UnspawnEventId,
				playerRespawnInfos.PreSpawnEventId,
				playerRespawnInfos.RespawningEventId
			});
			GameHubBehaviour.Hub.Events.ForgetEvent(playerRespawnInfos.DeathEventId);
			GameHubBehaviour.Hub.Events.ForgetEvent(playerRespawnInfos.UnspawnEventId);
			GameHubBehaviour.Hub.Events.ForgetEvent(playerRespawnInfos.PreSpawnEventId);
			GameHubBehaviour.Hub.Events.ForgetEvent(playerRespawnInfos.RespawningEventId);
			this.PlayerRespawnInfosDic.Remove(playerCarId);
		}

		public bool IsUnspawned(int id)
		{
			BaseSpawnManager.PlayerRespawnInfos playerRespawnInfos;
			return this.PlayerRespawnInfosDic.TryGetValue(id, out playerRespawnInfos);
		}

		public virtual void OnCleanup(CleanupMessage msg)
		{
			this._spawn = null;
			this.Players.Clear();
			this.BuildQueue.Clear();
			this.CreatedPlayers = 0;
			this.CarCreationFinished = false;
		}

		private void Death(PlayerEvent data, int eventId)
		{
			BaseSpawnManager.Log.InfoFormat("Death player={0} eventId={1} Position={2}", new object[]
			{
				data.TargetId,
				eventId,
				data.Location
			});
			for (int i = 0; i < GameHubBehaviour.Hub.Players.PlayersAndBots.Count; i++)
			{
				if (GameHubBehaviour.Hub.Players.PlayersAndBots[i].PlayerCarId == data.TargetId)
				{
					BaseSpawnManager.PlayerRespawnInfos value = default(BaseSpawnManager.PlayerRespawnInfos);
					value.DeathEventId = eventId;
					this.PlayerRespawnInfosDic[data.TargetId] = value;
					if (this.ListenToObjectDeath != null)
					{
						this.ListenToObjectDeath(data);
					}
					return;
				}
			}
		}

		public void Trigger(PlayerEvent player, int eventId)
		{
			switch (player.EventKind)
			{
			case PlayerEvent.Kind.Unspawn:
				this.Unspawn(player, eventId);
				break;
			case PlayerEvent.Kind.Respawn:
				this.Respawn(player, eventId);
				break;
			case PlayerEvent.Kind.PreRespawn:
				this.PreRespawn(player, eventId);
				break;
			case PlayerEvent.Kind.Respawning:
				this.Respawning(player, eventId);
				break;
			case PlayerEvent.Kind.Death:
				this.Death(player, eventId);
				break;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BaseSpawnManager));

		private LevelSpawn _spawn;

		public int RespawnTimeSeconds;

		protected readonly List<SpawnController> Players = new List<SpawnController>(8);

		protected readonly Dictionary<int, BaseSpawnManager.PlayerRespawnInfos> PlayerRespawnInfosDic = new Dictionary<int, BaseSpawnManager.PlayerRespawnInfos>(8);

		private TimedUpdater _update;

		protected readonly Dictionary<int, IFuture<Identifiable>> BuildQueue = new Dictionary<int, IFuture<Identifiable>>();

		protected int CreatedPlayers;

		protected bool FirstSpawnFinished;

		public struct PlayerRespawnInfos
		{
			public int UnspawnEventId;

			public int PreSpawnEventId;

			public int RespawningEventId;

			public int DeathEventId;

			public bool RespawningPositionSet;

			public bool SpawnPositionSet;

			public Vector3 Position;

			public Vector3 Forward;
		}

		public delegate void PlayerSpawnListener(PlayerEvent data);

		public delegate void PlayerUnspawnListener(PlayerEvent data);
	}
}
