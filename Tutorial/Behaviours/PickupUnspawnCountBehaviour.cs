using System;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	[RequireComponent(typeof(Identifiable))]
	[RemoteClass]
	public class PickupUnspawnCountBehaviour : InGameTutorialBehaviourBase, IBitComponent
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			GameHubBehaviour.Hub.Events.Pickups.ListenToPickupUnspawn += this.ListenToPickupUnspawn;
			this.Dispatch(GameHubBehaviour.Hub.SendAll).UpdatePickupInterfaceOnClient(this._pickupsCount);
		}

		private void ListenToPickupUnspawn(PickupRemoveEvent data)
		{
			if (GameHubBehaviour.Hub.Players.Players[0].CharacterInstance.Id.ObjId != data.Causer)
			{
				return;
			}
			this._pickupsCount++;
			this.Dispatch(GameHubBehaviour.Hub.SendAll).UpdatePickupInterfaceOnClient(this._pickupsCount);
			if (this.PickupsToCompleteBehaviour <= this._pickupsCount)
			{
				GameHubBehaviour.Hub.Events.Pickups.ListenToPickupUnspawn -= this.ListenToPickupUnspawn;
				this.CompleteBehaviourAndSync();
			}
		}

		[RemoteMethod]
		private void UpdatePickupInterfaceOnClient(int pickupsCounts)
		{
			this._pickupsCount = pickupsCounts;
			TutorialUIController.Instance.UpdateDescriptionParameter(new object[]
			{
				this._pickupsCount,
				this.PickupsToCompleteBehaviour
			});
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

		public IPickupUnspawnCountBehaviourAsync Async()
		{
			return this.Async(0);
		}

		public IPickupUnspawnCountBehaviourAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new PickupUnspawnCountBehaviourAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IPickupUnspawnCountBehaviourDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PickupUnspawnCountBehaviourDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IPickupUnspawnCountBehaviourDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PickupUnspawnCountBehaviourDispatch(this.OID);
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

		public object Invoke(int classId, short methodId, object[] args, BitStream bitstream = null)
		{
			this._delayed = null;
			if (methodId != 3)
			{
				throw new ScriptMethodNotFoundException(classId, (int)methodId);
			}
			this.UpdatePickupInterfaceOnClient((int)args[0]);
			return null;
		}

		public int PickupsToCompleteBehaviour;

		private int _pickupsCount;

		public const int StaticClassId = 1014;

		private Identifiable _identifiable;

		[ThreadStatic]
		private PickupUnspawnCountBehaviourAsync _async;

		[ThreadStatic]
		private PickupUnspawnCountBehaviourDispatch _dispatch;

		private IFuture _delayed;
	}
}
