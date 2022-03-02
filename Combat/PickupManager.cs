using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Scene;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Combat
{
	public class PickupManager : GameHubBehaviour, ICleanupListener
	{
		public static int GetPickupId(int eventId)
		{
			return ObjectId.New(ContentKind.Scrap.Byte(), eventId);
		}

		public void Trigger(IEventContent pickup, int eventId)
		{
			PickupManager.Log.DebugFormat("Received event={0} k={1} t={2}", new object[]
			{
				eventId,
				pickup.GetKind(),
				pickup.GetType()
			});
			PickupDropEvent pickupDropEvent = pickup as PickupDropEvent;
			if (pickupDropEvent != null)
			{
				this.Spawn(pickupDropEvent, eventId);
				return;
			}
			PickupRemoveEvent pickupRemoveEvent = pickup as PickupRemoveEvent;
			if (pickupRemoveEvent != null)
			{
				this.Unspawn(pickupRemoveEvent, eventId);
			}
		}

		private void Spawn(PickupDropEvent data, int eventId)
		{
			PickupManager.Log.DebugFormat("Pickup spawned={0} pos={1} asset={2}", new object[]
			{
				eventId,
				data.Position,
				data.PickupAsset
			});
			int pickupId = PickupManager.GetPickupId(eventId);
			if (this._missingPickups.ContainsKey(pickupId))
			{
				this._missingPickups.Remove(pickupId);
				return;
			}
			if (this._pickups.ContainsKey(pickupId))
			{
				return;
			}
			object obj = GameHubBehaviour.Hub.Resources.CacheInstantiate(data.PickupAsset, typeof(Transform), data.Position, Quaternion.identity);
			if (obj == null)
			{
				PickupManager.Log.ErrorFormat("Pickup null!", new object[]
				{
					data.Reason
				});
				return;
			}
			Transform transform = (Transform)obj;
			Identifiable component = transform.GetComponent<Identifiable>();
			component.transform.parent = GameHubBehaviour.Hub.Drawer.Pickups;
			component.gameObject.name = string.Concat(new object[]
			{
				"[",
				eventId,
				"-",
				pickupId,
				"] ",
				data.PickupAsset
			});
			component.Register(pickupId);
			BasePickup component2 = component.GetComponent<BasePickup>();
			if (component2)
			{
				component2.Setup(data);
			}
			this.SetupBombInfo(data, component, eventId);
			PickupManager.PickupInstance pickupInstance = new PickupManager.PickupInstance(component.Id.ObjId)
			{
				Pickup = component,
				DropEvent = data,
				EventId = eventId,
				Path = data.PickupAsset
			};
			this._pickups[pickupInstance.Id] = pickupInstance;
			this._pickupList.Add(pickupInstance);
			Mural.PostDeep(new SpawnEvent(component.Id.ObjId, data.Position, data.Reason), component);
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this.SendShow(pickupInstance);
			}
			if (this.ListenToPickupSpawn != null)
			{
				this.ListenToPickupSpawn(data, component.Id.ObjId);
			}
		}

		private void SetupBombInfo(PickupDropEvent data, Identifiable scrap, int eventId)
		{
			BombPickup component = scrap.GetComponent<BombPickup>();
			if (component != null)
			{
				GameHubBehaviour.Hub.BombManager.OnPickupCreated(data, component, eventId);
				if (GameHubBehaviour.Hub.Net.IsServer())
				{
					PickupManager.Log.InfoFormat("BombDropped causer={0} Reason={1} Killer={2} MatchTime={3}", new object[]
					{
						data.Causer,
						data.Reason,
						data.Killer,
						(float)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() / 1000f
					});
					MatchLogWriter.BombDropped(data.Causer, data.Reason, data.Position);
					MatchLogWriter.BombEvent(-1, 1, 0f, data.Position, -1f, false);
				}
				return;
			}
			BombDetonator component2 = scrap.GetComponent<BombDetonator>();
			if (component2 != null)
			{
				GameHubBehaviour.Hub.BombManager.OnDetonatorCreated(data, component2);
				if (GameHubBehaviour.Hub.Net.IsServer())
				{
					MatchLogWriter.WriteDamage();
				}
			}
		}

		private void Unspawn(PickupRemoveEvent data, int eventId)
		{
			PickupManager.Log.DebugFormat("Pickup Unspawned pickup={0} reason={1} position={2}", new object[]
			{
				data.PickupId.GetInstanceId(),
				data.Reason,
				data.Position
			});
			PickupManager.PickupInstance pickupInstance;
			if (!this._pickups.TryGetValue(data.PickupId, out pickupInstance))
			{
				this._missingPickups[data.PickupId] = data;
				return;
			}
			if (this.ListenToPickupUnspawn != null)
			{
				this.ListenToPickupUnspawn(data);
			}
			Mural.PostDeep(new UnspawnEvent(data.Position, data.Reason, data.Causer, -1), pickupInstance.Pickup);
			this._pickups.Remove(data.PickupId);
			this._pickupList.Remove(pickupInstance);
			int instanceId = data.PickupId.GetInstanceId();
			GameHubBehaviour.Hub.Events.ForgetEvent(instanceId);
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this.SendRemove(data, eventId);
			}
			this.CheckBombPickup(data, pickupInstance);
			GameHubBehaviour.Hub.Resources.ReturnToCache(pickupInstance.Path, pickupInstance.Pickup.transform);
		}

		private void CheckBombPickup(PickupRemoveEvent data, PickupManager.PickupInstance pickupInstance)
		{
			BombPickup component = pickupInstance.Pickup.GetComponent<BombPickup>();
			if (component != null)
			{
				GameHubBehaviour.Hub.BombManager.OnBombUnspawned(data, component);
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<PickupRemoveEvent> ListenToPickupUnspawn;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<PickupDropEvent, int> ListenToPickupSpawn;

		public PickupManager.PickupInstance GetPickUpByEventID(int eventId)
		{
			return this._pickupList.Find((PickupManager.PickupInstance p) => p.EventId == eventId);
		}

		public PickupManager.PickupInstance GetPickUpByID(int id)
		{
			PickupManager.PickupInstance pickupInstance;
			return (!this._pickups.TryGetValue(id, out pickupInstance)) ? null : pickupInstance;
		}

		private void SendShow(PickupManager.PickupInstance pickup)
		{
			this._eventDispatcher.Send(new EventData
			{
				Content = pickup.DropEvent,
				EventId = pickup.EventId,
				PreviousId = -1,
				Time = GameHubBehaviour.Hub.GameTime.GetPlaybackTime()
			});
		}

		private void SendRemove(PickupRemoveEvent data, int eventId)
		{
			this._eventDispatcher.Send(new EventData
			{
				Content = data,
				EventId = eventId,
				PreviousId = data.TargetEventId,
				Time = GameHubBehaviour.Hub.GameTime.GetPlaybackTime()
			});
		}

		public void OnCleanup(CleanupMessage msg)
		{
			this._pickupList.Clear();
			this._pickups.Clear();
			this._missingPickups.Clear();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PickupManager));

		[Inject]
		private IEventManagerDispatcher _eventDispatcher;

		public readonly List<PickupManager.PickupInstance> _pickupList = new List<PickupManager.PickupInstance>();

		private readonly Dictionary<int, PickupManager.PickupInstance> _pickups = new Dictionary<int, PickupManager.PickupInstance>();

		private readonly Dictionary<int, PickupRemoveEvent> _missingPickups = new Dictionary<int, PickupRemoveEvent>();

		[Serializable]
		public class PickupInstance
		{
			public PickupInstance(int id)
			{
				this.Id = id;
			}

			public PickupRemoveEvent HideEvent
			{
				get
				{
					if (this._hideEvent == null)
					{
						this._hideEvent = new PickupRemoveEvent
						{
							Causer = -1,
							PickupId = this.Id,
							Reason = SpawnReason.Hide,
							TargetEventId = -1
						};
					}
					this._hideEvent.Position = this.Pickup.transform.position;
					return this._hideEvent;
				}
			}

			public override int GetHashCode()
			{
				return this.Id.GetHashCode();
			}

			public readonly int Id;

			public string Path;

			public Identifiable Pickup;

			public PickupDropEvent DropEvent;

			public int EventId;

			private PickupRemoveEvent _hideEvent;
		}
	}
}
