using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Scene
{
	public abstract class BasePickupDropper : GameHubBehaviour, IObjectSpawnListener
	{
		protected CombatObject CombatObject
		{
			get
			{
				if (this._combatObject == null)
				{
					this._combatObject = base.GetComponent<CombatObject>();
				}
				return this._combatObject;
			}
		}

		public virtual void OnObjectUnspawned(UnspawnEvent evt)
		{
			if (this.DropOnObjectUnspawn)
			{
				this.Drop(SpawnReason.None, evt);
			}
		}

		public void Drop(SpawnReason spawnReason = SpawnReason.None, UnspawnEvent unspawnEvent = default(UnspawnEvent))
		{
			if (!this.BeforeDrop(spawnReason, unspawnEvent))
			{
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				this.OnClientDrop(spawnReason, unspawnEvent);
			}
			else
			{
				this.OnServerDrop(spawnReason, unspawnEvent);
			}
			this.AfterDrop(spawnReason, unspawnEvent);
		}

		protected abstract bool BeforeDrop(SpawnReason spawnReason, UnspawnEvent unspawnEvent);

		private void OnClientDrop(SpawnReason spawnReason, UnspawnEvent unspawnEvent)
		{
			this.ExecuteOnClientDrop(spawnReason, unspawnEvent);
		}

		protected abstract void ExecuteOnClientDrop(SpawnReason spawnReason, UnspawnEvent unspawnEvent);

		private void OnServerDrop(SpawnReason spawnReason, UnspawnEvent unspawnEvent)
		{
			if (this.ExecuteOnServerDrop(spawnReason, unspawnEvent))
			{
				int num = 1;
				PickupDropEvent pickupDropEvent = this.CreatePickupDropEvent(spawnReason, unspawnEvent);
				for (int i = 0; i < num; i++)
				{
					Vector3 b = UnityEngine.Random.insideUnitCircle.normalized.ToVector3XZ() * this.DropDistance * (float)i;
					PickupDropEvent pickupDropEvent2 = pickupDropEvent.Clone();
					pickupDropEvent2.Position += b;
					GameHubBehaviour.Hub.Events.TriggerEvent(pickupDropEvent2);
				}
			}
		}

		protected abstract bool ExecuteOnServerDrop(SpawnReason spawnReason, UnspawnEvent unspawnEvent);

		protected abstract PickupDropEvent CreatePickupDropEvent(SpawnReason spawnReason, UnspawnEvent unspawnEvent);

		protected abstract void AfterDrop(SpawnReason spawnReason, UnspawnEvent unspawnEvent);

		public virtual void OnObjectSpawned(SpawnEvent evt)
		{
		}

		private CombatObject _combatObject;

		public string AssetPickup;

		public float DropDistance = 1f;

		public bool UnspawnOnLifeTimeEnd = true;

		public bool DropOnObjectUnspawn = true;

		protected int PickupEventsPerDrop = 1;
	}
}
