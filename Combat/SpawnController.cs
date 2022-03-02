using System;
using System.Diagnostics;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Playback.Snapshot;
using HeavyMetalMachines.UpdateStream;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class SpawnController : StreamContent, ISpawnControllerSerialData, IBaseStreamSerialData<ISpawnControllerSerialData>
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
		public event Action<SpawnStateKind> OnStateChanged;

		public SpawnStateKind State
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
			bool flag = GameHubBehaviour.Hub.Net.IsServer();
			SpawnController.Log.DebugFormat("Show {0}. Renderer {1} RenderParent {2} LifeBar {3}", new object[]
			{
				isEnabled,
				this.Renderer,
				this.Renderer.transform.parent,
				this.m_oLifebar
			});
			if (this.Renderer)
			{
				if (flag)
				{
					this.Renderer.gameObject.SetActive(isEnabled);
				}
				else
				{
					this.EnableRendererInClient(isEnabled);
				}
			}
			if (this.m_oLifebar && (!flag || !isEnabled))
			{
				this.m_oLifebar.SetActive(isEnabled);
			}
			else if (isEnabled)
			{
				SpawnController.Log.DebugFormat("Lifebar not enabled: Lifebar {0} IsServer {1}", new object[]
				{
					this.m_oLifebar,
					flag
				});
			}
		}

		private void EnableRendererInClient(bool isEnabled)
		{
			if (isEnabled && !this._isHidden)
			{
				this.Renderer.gameObject.SetActive(isEnabled);
			}
			else if (isEnabled && this._isHidden)
			{
				GameObject gameObject = this.Renderer.gameObject;
				Vector3 localPosition = gameObject.transform.localPosition;
				localPosition.y = 0f;
				gameObject.transform.localPosition = localPosition;
				this._isHidden = false;
			}
			else if (!isEnabled)
			{
				GameObject gameObject2 = this.Renderer.gameObject;
				Vector3 localPosition2 = gameObject2.transform.localPosition;
				localPosition2.y = 10000f;
				gameObject2.transform.localPosition = localPosition2;
				this._isHidden = true;
			}
		}

		public bool IsSpawned()
		{
			return this.State == SpawnStateKind.Spawned;
		}

		public bool IsUnspawned()
		{
			return this.State == SpawnStateKind.Unspawned;
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
			if (this.State != SpawnStateKind.Respawning)
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
			return (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState != BombScoreboardState.BombDelivery) ? this.StartPosition : this.SpawnPosition;
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
			this.State = SpawnStateKind.PreSpawned;
		}

		public void Respawning()
		{
			this.RespawningTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this.State = SpawnStateKind.Respawning;
		}

		public void Spawn(Vector3 pos, Vector3 dir, int spawnEventId, SpawnReason reason)
		{
			SpawnController.Log.DebugFormat("Spawn ObjId={0} Position={1} name={2}", new object[]
			{
				base.Id.ObjId,
				pos,
				base.name
			});
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
			this.State = SpawnStateKind.Spawned;
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

		public void Unspawn(Vector3 pos, SpawnReason reason, int causerId, int targetId)
		{
			this.SpawnTime = -1;
			this.UnspawnTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this.State = SpawnStateKind.Unspawned;
			UnspawnEvent unspawnEvent = new UnspawnEvent(pos, reason, causerId, targetId);
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
			if (!component || (component && component.HideOnUnspawn))
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
			if (this.Combat && (this.Combat.IsPlayer || this.Combat.IsBot))
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
			IGameArenaInfo currentArena = GameHubBehaviour.Hub.ArenaConfig.GetCurrentArena();
			this._respawnTimeMillis = (int)(currentArena.RespawnTimeSeconds * 1000f);
			this._respawningTimeMillis = (int)(currentArena.RespawningTimeSeconds * 1000f);
		}

		public override int GetStreamData(ref byte[] data, bool boForceSerialization)
		{
			BitStream writeStream = StaticBitStream.GetWriteStream();
			writeStream.WriteCompressedInt((int)this.State);
			writeStream.WriteCompressedInt(this.SpawnTime);
			writeStream.WriteCompressedInt(this.UnspawnTime);
			return writeStream.CopyToArray(data);
		}

		public override void ApplyStreamData(byte[] data)
		{
			BitStream readStream = StaticBitStream.GetReadStream(data);
			this._state = (SpawnStateKind)readStream.ReadCompressedInt();
			this.SpawnTime = readStream.ReadCompressedInt();
			this.UnspawnTime = readStream.ReadCompressedInt();
		}

		public void Apply(ISpawnControllerSerialData other)
		{
			this._state = other.State;
			this.SpawnTime = other.SpawnTime;
			this.UnspawnTime = other.UnspawnTime;
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

		private SpawnStateKind _state;

		private int _unspawnTime;

		private int _respawnTimeMillis;

		private int _respawningTimeMillis;

		private bool _forceFinishDeathTime;

		private bool _isHidden;

		private const float HideOffset = 10000f;
	}
}
