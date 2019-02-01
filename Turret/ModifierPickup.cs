using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Turret
{
	[Obsolete]
	public class ModifierPickup : GameHubBehaviour, ICachedObject
	{
		private void Awake()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				if (this.Modifier.Info.Feedback != null)
				{
					GameHubBehaviour.Hub.Resources.PreCachePrefab(this.Modifier.Info.Feedback.Name, this.Modifier.Info.Feedback.EffectPreCacheCount);
				}
				base.enabled = false;
				return;
			}
			this.Modifier.SetInfo(this.Modifier.Info, null);
		}

		private void Update()
		{
			if (this.Picked || this._endTime <= 0 || GameHubBehaviour.Hub.GameTime.GetPlaybackTime() < this._endTime)
			{
				return;
			}
			GameHubBehaviour.Hub.Events.TriggerEvent(new PickupRemoveEvent
			{
				Causer = -1,
				PickupId = base.Id.ObjId,
				Position = base.transform.position,
				Reason = SpawnReason.LifeTime
			});
			this.Picked = true;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (this.Picked)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this.ServerTriggerEnter(other);
			}
		}

		private void ServerTriggerEnter(Collider other)
		{
			if (this.Picked)
			{
				return;
			}
			CombatObject combat = CombatRef.GetCombat(other);
			if (!combat || !combat.IsPlayer)
			{
				return;
			}
			combat.Controller.AddModifier(this.Modifier, null, -1, false);
			GameHubBehaviour.Hub.Events.TriggerEvent(new PickupRemoveEvent
			{
				Causer = combat.Id.ObjId,
				PickupId = base.Id.ObjId,
				Position = base.transform.position,
				Reason = SpawnReason.Pickup
			});
			this.Picked = true;
		}

		public void OnSendToCache()
		{
		}

		public void OnGetFromCache()
		{
			this.Picked = false;
			if (this.LifeTimeSeconds > 0f)
			{
				this._endTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime() + (int)(this.LifeTimeSeconds * 1000f);
			}
			else
			{
				this._endTime = -1;
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ModifierPickup));

		public ModifierData Modifier;

		public float LifeTimeSeconds;

		public bool Picked;

		private int _endTime;
	}
}
