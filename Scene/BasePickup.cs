using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Scene
{
	public abstract class BasePickup : GameHubBehaviour, ICachedObject
	{
		private void OnTriggerEnter(Collider other)
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this.ServerTriggerEnter(other);
			}
		}

		private void Update()
		{
			if (this.Picked || this._endTime < 0 || GameHubBehaviour.Hub.GameTime.GetPlaybackTime() < this._endTime || !this.UnspawnOnLifeTimeEnd)
			{
				return;
			}
			GameHubBehaviour.Hub.Events.TriggerEvent(new PickupRemoveEvent
			{
				Causer = -1,
				PickupId = base.Id.ObjId,
				Position = base.transform.position,
				Reason = SpawnReason.LifeTime,
				TargetEventId = -1
			});
			this.Picked = true;
		}

		protected virtual void ServerTriggerEnter(Collider other)
		{
			if (this.Picked)
			{
				return;
			}
			CombatObject combat = CombatRef.GetCombat(other);
			if (!combat || !combat.IsPlayer || !combat.IsAlive())
			{
				return;
			}
			if (this.PickerTeam != TeamKind.Zero && this.PickerTeam != TeamKind.Neutral && combat.Team != this.PickerTeam)
			{
				return;
			}
			this.RemovePickupAsset(combat.Id.ObjId);
			this.Picked = true;
			this.ExecuteOnTriggerServer(combat, other);
		}

		protected abstract void ExecuteOnTriggerServer(CombatObject combatObject, Collider other);

		public void RemovePickupAsset(int id)
		{
			GameHubBehaviour.Hub.Events.TriggerEvent(new PickupRemoveEvent
			{
				Causer = id,
				PickupId = base.Id.ObjId,
				Position = base.transform.position,
				Reason = SpawnReason.Pickup,
				TargetEventId = -1
			});
		}

		public virtual void Setup(PickupDropEvent pickupDropEvent)
		{
			this.Picked = false;
			this.PickerTeam = pickupDropEvent.PickerTeam;
			this.UnspawnOnLifeTimeEnd = pickupDropEvent.UnspawnOnLifeTimeEnd;
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				if (this.FriendlyFX)
				{
					this.FriendlyFX.SetActive(this.PickerTeam == GameHubBehaviour.Hub.Players.CurrentPlayerData.Team);
				}
				if (this.EnemyFX)
				{
					this.EnemyFX.SetActive(this.PickerTeam != GameHubBehaviour.Hub.Players.CurrentPlayerData.Team && this.PickerTeam != TeamKind.Zero && this.PickerTeam != TeamKind.Neutral);
				}
			}
		}

		public void OnSendToCache()
		{
		}

		public virtual void OnGetFromCache()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.Picked = false;
			if (GameHubBehaviour.Hub.ScrapLevel.ScrapLifeTime > 0)
			{
				this._endTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime() + GameHubBehaviour.Hub.ScrapLevel.ScrapLifeTime * 1000;
			}
			else
			{
				this._endTime = -1;
			}
		}

		public GameObject FriendlyFX;

		public GameObject EnemyFX;

		public bool Picked;

		protected TeamKind PickerTeam;

		private int _endTime;

		public bool UnspawnOnLifeTimeEnd;

		[Tooltip("If is Red or Blue just players from the same team can get it!")]
		protected TeamKind Team;
	}
}
