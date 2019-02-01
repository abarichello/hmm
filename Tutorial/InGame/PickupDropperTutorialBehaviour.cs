using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.InGame
{
	[RemoteClass]
	[RequireComponent(typeof(Identifiable))]
	public class PickupDropperTutorialBehaviour : InGameTutorialBehaviourBase, IBitComponent
	{
		private int RemovedPickupsCount
		{
			get
			{
				return this._removedPickupsCount;
			}
			set
			{
				string text = string.Concat(new object[]
				{
					"[ ",
					value,
					"/",
					this.Dummies.Length,
					" ]"
				});
				this.SendScrapUpdateToClient(text);
				Debug.Log(string.Concat(new object[]
				{
					"Scrap String to send ",
					text,
					" - Value:",
					value
				}));
				this._removedPickupsCount = value;
			}
		}

		protected override void StartBehaviourOnClient()
		{
			Debug.Log(string.Format("PickUpDropper Started", new object[0]), base.gameObject);
			base.SetPlayerInputsActive(true);
		}

		protected override void StartBehaviourOnServer()
		{
			Debug.Log(string.Format("PickUpDropper StartBehaviourOnServer", new object[0]), base.gameObject);
			for (int i = 0; i < this.Dummies.Length; i++)
			{
				GameHubBehaviour.Hub.Events.TriggerEvent(new PickupDropEvent
				{
					Position = this.Dummies[i].position,
					PickupAsset = this.Pickup,
					UnspawnOnLifeTimeEnd = false
				}, new Action<int>(this.OnScrapSpawnDone));
			}
			this.RemovedPickupsCount = 0;
			base.SetPlayerInputsActive(true);
		}

		public void SendScrapUpdateToClient(string scrapText)
		{
			Debug.Log(string.Format("PickUpDropper SendScrapUpdateToClient", new object[0]), base.gameObject);
			this.DispatchReliable(GameHubBehaviour.Hub.SendAll).SetInterfaceScraps(scrapText);
		}

		[RemoteMethod]
		public void SetInterfaceScraps(string scrapText)
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			Debug.Log("Client Scrap text received " + scrapText);
			TutorialUIController.Instance.UpdateDescriptionParameter(new object[]
			{
				scrapText
			});
		}

		protected override void UpdateOnServer()
		{
			base.UpdateOnServer();
			if (!this._active)
			{
				return;
			}
			for (int i = 0; i < this.ActivePickups.Count; i++)
			{
				if (!this.ActivePickups[i].Pickup.gameObject.activeInHierarchy)
				{
					this.ActivePickups.RemoveAt(i);
					i--;
					this.RemovedPickupsCount++;
				}
			}
			if (this.ActivePickups.Count != 0)
			{
				return;
			}
			this._active = false;
			base.SetPlayerInputsActive(false);
			this.CompleteBehaviourAndSync();
		}

		private void OnScrapSpawnDone(int eventId)
		{
			PickupManager.PickupInstance pickUpByEventID = GameHubBehaviour.Hub.Events.Pickups.GetPickUpByEventID(eventId);
			if (pickUpByEventID == null)
			{
				return;
			}
			this.ActivePickups.Add(pickUpByEventID);
			if (this.ActivePickups.Count == this.Dummies.Length)
			{
				this.RemovedPickupsCount = 0;
				this._active = true;
			}
		}

		private int OID
		{
			get
			{
				if (!this._identifiable)
				{
					this._identifiable = base.GetComponent<Identifiable>();
				}
				return this._identifiable.ObjId;
			}
		}

		public byte Sender { get; set; }

		public IPickupDropperTutorialBehaviourAsync Async()
		{
			return this.Async(0);
		}

		public IPickupDropperTutorialBehaviourAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new PickupDropperTutorialBehaviourAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IPickupDropperTutorialBehaviourDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PickupDropperTutorialBehaviourDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IPickupDropperTutorialBehaviourDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PickupDropperTutorialBehaviourDispatch(this.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		protected IFuture Delayed
		{
			get
			{
				return this._delayed;
			}
		}

		protected void Delay(IFuture future)
		{
			this._delayed = future;
		}

		public object Invoke(int classId, short methodId, object[] args)
		{
			if (classId != 1012)
			{
				throw new Exception("Hierarchy in RemoteClass is not allowed!!! " + classId);
			}
			this._delayed = null;
			if (methodId != 4)
			{
				throw new ScriptMethodNotFoundException(classId, (int)methodId);
			}
			this.SetInterfaceScraps((string)args[0]);
			return null;
		}

		public string Pickup;

		public Transform[] Dummies;

		private bool _active;

		public List<PickupManager.PickupInstance> ActivePickups = new List<PickupManager.PickupInstance>();

		private int _removedPickupsCount;

		public const int StaticClassId = 1012;

		private Identifiable _identifiable;

		[ThreadStatic]
		private PickupDropperTutorialBehaviourAsync _async;

		[ThreadStatic]
		private PickupDropperTutorialBehaviourDispatch _dispatch;

		private IFuture _delayed;
	}
}
