using System;
using System.Collections;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Scene
{
	public class ActivatedScenerySpawn : GameHubBehaviour, IActivatable
	{
		public void Activate(bool enable, int causer)
		{
			if (!enable || !base.enabled)
			{
				return;
			}
			Identifiable @object = GameHubBehaviour.Hub.ObjectCollection.GetObject(causer);
			if (@object == null)
			{
				return;
			}
			CombatObject component = @object.GetComponent<CombatObject>();
			if (component == null)
			{
				return;
			}
			if (this.SpawnDelaySeconds > 0f)
			{
				base.StartCoroutine(this.DelayedTriggerSpawn());
			}
			else
			{
				this.TriggerSpawn();
			}
		}

		private IEnumerator DelayedTriggerSpawn()
		{
			yield return this.waitForSpawnDelay;
			this.TriggerSpawn();
			yield break;
		}

		private void TriggerSpawn()
		{
			if (this.Scenery.Combat.IsAlive())
			{
				return;
			}
			GameHubBehaviour.Hub.Events.TriggerEvent(new SceneEvent
			{
				CauserId = this.Scenery.Id.ObjId,
				SceneObjectId = this.Scenery.Id.ObjId,
				State = SceneEvent.StateKind.Spawned
			});
			this.ThrowAnnounceEvent();
		}

		private void ThrowAnnounceEvent()
		{
			if (this.LogKind == AnnouncerLog.AnnouncerEventKinds.None)
			{
				return;
			}
			AnnouncerEvent content = new AnnouncerEvent
			{
				AnnouncerEventKind = this.LogKind
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
		}

		private void OnDestroy()
		{
			if (this.Scenery)
			{
				this.Scenery.OnUnspawn -= this.OnObjectUnspawned;
			}
		}

		private void Awake()
		{
			if (this.Scenery)
			{
				this.Scenery.OnUnspawn += this.OnObjectUnspawned;
			}
			this.waitForSpawnDelay = new WaitForSeconds(this.SpawnDelaySeconds);
		}

		private void Start()
		{
			if (!this.Scenery)
			{
				ActivatedScenerySpawn.Log.ErrorFormat("ActivatedScenerySpawn={0} Missing Scenery", new object[]
				{
					base.name
				});
				base.enabled = false;
				return;
			}
			GameHubBehaviour.Hub.Events.TriggerEvent(new SceneEvent
			{
				SceneObjectId = this.Scenery.Id.ObjId,
				State = SceneEvent.StateKind.Unspawned,
				CauserId = -1
			});
		}

		public void OnObjectUnspawned(UnspawnEvent evt)
		{
		}

		public void DestroyScenery()
		{
			if (!base.enabled)
			{
				return;
			}
			GameHubBehaviour.Hub.Events.TriggerEvent(new SceneEvent
			{
				SceneObjectId = this.Scenery.Id.ObjId,
				State = SceneEvent.StateKind.Unspawned,
				CauserId = -1
			});
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ActivatedScenerySpawn));

		public float SpawnDelaySeconds;

		public SpawnController Scenery;

		public AnnouncerLog.AnnouncerEventKinds LogKind;

		private WaitForSeconds waitForSpawnDelay;
	}
}
