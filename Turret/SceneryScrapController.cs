using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Turret
{
	[Obsolete]
	public class SceneryScrapController : GameHubBehaviour, IObjectSpawnListener
	{
		private void Awake()
		{
			this.BoxCollider = base.gameObject.GetComponent<BoxCollider>();
		}

		private void Start()
		{
			if (!GameHubBehaviour.Hub)
			{
				return;
			}
			if (!GameHubBehaviour.Hub.Net.IsClient())
			{
				this.MaxTimeDeadState = GameHubBehaviour.Hub.ScrapLevel.ScrapLifeTime;
			}
			this.Enable();
		}

		public void Disable()
		{
			this.IsAlive = false;
			this.DeadStateTimer = (float)(GameHubBehaviour.Hub.GameTime.GetPlaybackTime() + this.MaxTimeDeadState * 1000);
			this.BoxCollider.enabled = false;
			this.MyRenderer.SetActive(false);
		}

		public void Enable()
		{
			this._respawnTriggered = false;
			this.IsAlive = true;
			this.DeadStateTimer = 0f;
			this.BoxCollider.enabled = true;
			this.MyRenderer.SetActive(true);
		}

		private void Update()
		{
			if (!GameHubBehaviour.Hub)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				this.UpdateClient();
			}
			else
			{
				this.UpdateServer();
			}
		}

		private void UpdateServer()
		{
			if (!this.IsAlive && !this._respawnTriggered && this.DeadStateTimer <= (float)GameHubBehaviour.Hub.GameTime.GetPlaybackTime())
			{
				this._respawnTriggered = true;
				SceneEvent sceneEvent = new SceneEvent();
				sceneEvent.SceneObjectId = base.Id.ObjId;
				sceneEvent.State = SceneEvent.StateKind.Spawned;
				sceneEvent.CauserId = -1;
				GameHubBehaviour.Hub.Events.TriggerEvent(sceneEvent);
			}
		}

		public void OnObjectSpawned(SpawnEvent evt)
		{
		}

		public void OnObjectUnspawned(UnspawnEvent evt)
		{
		}

		private void UpdateClient()
		{
		}

		private void OnTriggerEnter(Collider other)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				this.ClientTriggerEnter(other);
			}
			else
			{
				this.ServerTriggerEnter(other);
			}
		}

		private void ClientTriggerEnter(Collider other)
		{
		}

		private void ServerTriggerEnter(Collider other)
		{
			CombatObject combat = CombatRef.GetCombat(other);
			if (combat && GameHubBehaviour.Hub.ObjectCollection.GetObjects().ContainsKey(combat.Id.ObjId))
			{
				SceneEvent sceneEvent = new SceneEvent();
				sceneEvent.SceneObjectId = base.Id.ObjId;
				sceneEvent.State = SceneEvent.StateKind.ScrapUnspawned;
				sceneEvent.CauserId = combat.Id.ObjId;
				GameHubBehaviour.Hub.Events.TriggerEvent(sceneEvent);
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(SceneryScrapController));

		public bool IsAlive;

		public int CauserId;

		public int MaxTimeDeadState;

		public float DeadStateTimer;

		public Collider BoxCollider;

		public GameObject MyRenderer;

		private bool _respawnTriggered;
	}
}
