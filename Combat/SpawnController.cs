using System;
using System.Diagnostics;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.UpdateStream;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class SpawnController : StreamContent
	{
		public GameObject Lifebar
		{
			get
			{
				return this.m_oLifebar;
			}
			set
			{
				this.m_oLifebar = value;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<SpawnController.StateType> OnStateChanged;

		public SpawnController.StateType State
		{
			get
			{
				return this._state;
			}
			set
			{
				if (this._state != value)
				{
					this._state = value;
					GameHubBehaviour.Hub.Stream.SpawnControllerStream.Changed(this);
					if (this.OnStateChanged != null)
					{
						this.OnStateChanged(this._state);
					}
				}
			}
		}

		public int UnspawnTime
		{
			get
			{
				return this._unspawnTime;
			}
			set
			{
				if (this._unspawnTime != value)
				{
					GameHubBehaviour.Hub.Stream.SpawnControllerStream.Changed(this);
				}
				this._unspawnTime = value;
			}
		}

		public int SpawnTime { get; private set; }

		public void EnableRenderer(bool isEnabled)
		{
			if (this.Renderer)
			{
				this.Renderer.gameObject.SetActive(isEnabled);
			}
			bool flag = GameHubBehaviour.Hub.Net.IsServer();
			if (this.m_oLifebar && (!flag || !isEnabled))
			{
				this.m_oLifebar.SetActive(isEnabled);
			}
			else if (isEnabled)
			{
			}
		}

		public bool IsSpawned()
		{
			return this.State == SpawnController.StateType.Spawned;
		}

		public bool IsUnspawned()
		{
			return this.State == SpawnController.StateType.Unspawned;
		}

		public int GetDeathTimeRemainingMillis()
		{
			if (!this.IsUnspawned())
			{
				return 0;
			}
			if (this.Combat && this.Combat.IsPlayer)
			{
				return Mathf.Max(this.UnspawnTime + this.GetPlayerMaxTimeDeadMillis() - GameHubBehaviour.Hub.GameTime.GetPlaybackTime(), 0);
			}
			return Mathf.Max(this.UnspawnTime + this.RespawnTimeMillis - GameHubBehaviour.Hub.GameTime.GetPlaybackTime(), 0);
		}

		public int GetPlayerMaxTimeDeadMillis()
		{
			return this._respawnTimeMillis;
		}

		public int GetRespawningRemainingMillis()
		{
			if (this.State != SpawnController.StateType.Respawning)
			{
				return 0;
			}
			return Mathf.Max(this.RespawningTime + this.GetRespawningMaxTimeMillis() - GameHubBehaviour.Hub.GameTime.GetPlaybackTime(), 0);
		}

		public int GetRespawningMaxTimeMillis()
		{
			return this._respawningTimeMillis;
		}

		public Transform GetSpawn()
		{
			return (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState != BombScoreBoard.State.BombDelivery) ? this.StartPosition : this.SpawnPosition;
		}

		public bool ShouldFinishDeathTime()
		{
			return this._forceFinishDeathTime || (this.RespawnTimeMillis >= 0 && this.GetDeathTimeRemainingMillis() <= 0);
		}

		public void ForceFinishDeathTime()
		{
			this._forceFinishDeathTime = true;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<SpawnEvent> OnSpawn;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<UnspawnEvent> OnUnspawn;

		public void PreSpawn()
		{
			this.State = SpawnController.StateType.PreSpawned;
		}

		public void Respawning()
		{
			this.RespawningTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this.State = SpawnController.StateType.Respawning;
		}

		public void Spawn(Vector3 pos, Vector3 dir, int spawnEventId, SpawnReason reason)
		{
			this._forceFinishDeathTime = false;
			this.SpawnTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this.UnspawnTime = -1;
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.transform.position = pos;
			}
			else
			{
				this.Combat.Movement.ForcePosition(pos, true);
			}
			base.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
			this.State = SpawnController.StateType.Spawned;
			this.EnableRenderer(true);
			CombatObject component = base.gameObject.GetComponent<CombatObject>();
			SpawnEvent spawnEvent = new SpawnEvent(component.Id.ObjId, pos, reason);
			foreach (MonoBehaviour monoBehaviour in base.GetComponentsInChildren<MonoBehaviour>(true))
			{
				if (monoBehaviour is IObjectSpawnListener)
				{
					((IObjectSpawnListener)monoBehaviour).OnObjectSpawned(spawnEvent);
				}
			}
			if (this.OnSpawn != null)
			{
				this.OnSpawn(spawnEvent);
			}
			if (this.ColliderObject)
			{
				this.ColliderObject.enabled = true;
			}
			this.LastSpawnEventId = spawnEventId;
		}

		public void Unspawn(Vector3 pos, SpawnReason reason, int causerId)
		{
			this.SpawnTime = -1;
			this.UnspawnTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this.State = SpawnController.StateType.Unspawned;
			UnspawnEvent unspawnEvent = new UnspawnEvent(pos, reason, causerId);
			foreach (MonoBehaviour monoBehaviour in base.GetComponentsInChildren<MonoBehaviour>(true))
			{
				if (monoBehaviour is IObjectSpawnListener)
				{
					((IObjectSpawnListener)monoBehaviour).OnObjectUnspawned(unspawnEvent);
				}
			}
			if (this.OnUnspawn != null)
			{
				this.OnUnspawn(unspawnEvent);
			}
			CombatObject component = base.gameObject.GetComponent<CombatObject>();
			if (!component || (component && !component.IsCreep && component.HideOnUnspawn))
			{
				this.EnableRenderer(false);
				if (this.ColliderObject)
				{
					this.ColliderObject.enabled = false;
				}
			}
			if (this.LastSpawnEventId > 0)
			{
				GameHubBehaviour.Hub.Events.ForgetEvent(this.LastSpawnEventId);
				this.LastSpawnEventId = -1;
			}
		}

		private void Start()
		{
			if (!GameHubBehaviour.Hub)
			{
				return;
			}
			if (this.Combat && (this.Combat.IsPlayer || this.Combat.IsBot || this.Combat.IsCreep))
			{
				this.ApplyArenaConfig();
			}
			if (base.Id.Mode == Identifiable.IdentifiableMode.Prefab)
			{
				return;
			}
		}

		private void ApplyArenaConfig()
		{
			int arenaIndex = GameHubBehaviour.Hub.Match.ArenaIndex;
			GameArenaInfo gameArenaInfo = GameHubBehaviour.Hub.ArenaConfig.Arenas[arenaIndex];
			this._respawnTimeMillis = (int)(gameArenaInfo.RespawnTimeSeconds * 1000f);
			this._respawningTimeMillis = (int)(gameArenaInfo.RespawningTimeSeconds * 1000f);
		}

		public override int GetStreamData(ref byte[] data, bool boForceSerialization)
		{
			Pocketverse.BitStream writeStream = StaticBitStream.GetWriteStream();
			writeStream.WriteCompressedInt((int)this.State);
			writeStream.WriteCompressedInt(this.SpawnTime);
			writeStream.WriteCompressedInt(this.UnspawnTime);
			return writeStream.CopyToArray(data);
		}

		public override void ApplyStreamData(byte[] data)
		{
			Pocketverse.BitStream readStream = StaticBitStream.GetReadStream(data);
			this._state = (SpawnController.StateType)readStream.ReadCompressedInt();
			this.SpawnTime = readStream.ReadCompressedInt();
			this.UnspawnTime = readStream.ReadCompressedInt();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(SpawnController));

		private GameObject m_oLifebar;

		public GameObject RenderObject;

		public Transform Renderer;

		public Transform RendererVisualGadgets;

		public string RenderObjectAsset;

		public string VisualGadgetsAsset;

		public Collider ColliderObject;

		public int RespawningTime;

		private int LastSpawnEventId = -1;

		public int RespawnTimeMillis;

		public PlayerData Player;

		public CombatObject Combat;

		public PlayerStats Scraps;

		public Transform StartPosition;

		public Transform SpawnPosition;

		private SpawnController.StateType _state;

		private int _unspawnTime;

		private int _respawnTimeMillis;

		private int _respawningTimeMillis;

		private bool _forceFinishDeathTime;

		public enum StateType : byte
		{
			None,
			Spawned,
			Unspawned,
			Pooled,
			PreSpawned,
			Respawning
		}
	}
}
