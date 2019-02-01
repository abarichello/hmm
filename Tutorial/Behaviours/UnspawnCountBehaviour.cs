using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	[RemoteClass]
	[RequireComponent(typeof(Identifiable))]
	public class UnspawnCountBehaviour : InGameTutorialBehaviourBase, IBitComponent
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			this.RegisterListener();
			this.DispatchReliable(GameHubBehaviour.Hub.SendAll).UpdateInterfaceOnClient(this._unspawnCount);
		}

		private void ListenToUnspawn(IEventContent data)
		{
			this._unspawnCount++;
			this.DispatchReliable(GameHubBehaviour.Hub.SendAll).UpdateInterfaceOnClient(this._unspawnCount);
			if (this.UnspawnsToCompleteBehaviour <= this._unspawnCount)
			{
				this.UnRegisterListener();
				this.CompleteBehaviourAndSync();
			}
		}

		private int GetCauserId(IEventContent data)
		{
			switch (this.UnspawnType)
			{
			case UnspawnCountBehaviour.EUnspawnType.Creep:
				return ((CreepRemoveEvent)data).CauserId;
			case UnspawnCountBehaviour.EUnspawnType.Bot:
				return ((BotAIEvent)data).CauserId;
			case UnspawnCountBehaviour.EUnspawnType.Pickup:
				return ((PickupRemoveEvent)data).Causer;
			default:
				return -1;
			}
		}

		[RemoteMethod]
		private void UpdateInterfaceOnClient(int pickupsCounts)
		{
			this._unspawnCount = pickupsCounts;
			TutorialUIController.Instance.UpdateDescriptionParameter(new object[]
			{
				this._unspawnCount,
				this.UnspawnsToCompleteBehaviour
			});
		}

		private void RegisterListener()
		{
			UnspawnCountBehaviour.EUnspawnType unspawnType = this.UnspawnType;
			if (unspawnType != UnspawnCountBehaviour.EUnspawnType.Pickup)
			{
				if (unspawnType != UnspawnCountBehaviour.EUnspawnType.Creep)
				{
					if (unspawnType == UnspawnCountBehaviour.EUnspawnType.Bot)
					{
						GameHubBehaviour.Hub.Events.Bots.ListenToObjectDeath += new BaseSpawnManager.PlayerUnspawnListener(this.ListenToUnspawn);
					}
				}
				else
				{
					GameHubBehaviour.Hub.Events.Creeps.ListenToCreepUnspawn += new CreepSpawnManager.CreepUnspawnListener(this.ListenToUnspawn);
				}
			}
			else
			{
				GameHubBehaviour.Hub.Events.Pickups.ListenToPickupUnspawn += new Action<PickupRemoveEvent>(this.ListenToUnspawn);
			}
		}

		private void UnRegisterListener()
		{
			UnspawnCountBehaviour.EUnspawnType unspawnType = this.UnspawnType;
			if (unspawnType != UnspawnCountBehaviour.EUnspawnType.Pickup)
			{
				if (unspawnType != UnspawnCountBehaviour.EUnspawnType.Creep)
				{
					if (unspawnType == UnspawnCountBehaviour.EUnspawnType.Bot)
					{
						GameHubBehaviour.Hub.Events.Bots.ListenToObjectDeath -= new BaseSpawnManager.PlayerUnspawnListener(this.ListenToUnspawn);
					}
				}
				else
				{
					GameHubBehaviour.Hub.Events.Creeps.ListenToCreepUnspawn -= new CreepSpawnManager.CreepUnspawnListener(this.ListenToUnspawn);
				}
			}
			else
			{
				GameHubBehaviour.Hub.Events.Pickups.ListenToPickupUnspawn -= new Action<PickupRemoveEvent>(this.ListenToUnspawn);
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

		public IUnspawnCountBehaviourAsync Async()
		{
			return this.Async(0);
		}

		public IUnspawnCountBehaviourAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new UnspawnCountBehaviourAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IUnspawnCountBehaviourDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new UnspawnCountBehaviourDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IUnspawnCountBehaviourDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new UnspawnCountBehaviourDispatch(this.OID);
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
			if (classId != 1015)
			{
				throw new Exception("Hierarchy in RemoteClass is not allowed!!! " + classId);
			}
			this._delayed = null;
			if (methodId != 4)
			{
				throw new ScriptMethodNotFoundException(classId, (int)methodId);
			}
			this.UpdateInterfaceOnClient((int)args[0]);
			return null;
		}

		public UnspawnCountBehaviour.EUnspawnType UnspawnType;

		public int UnspawnsToCompleteBehaviour;

		private int _unspawnCount;

		public const int StaticClassId = 1015;

		private Identifiable _identifiable;

		[ThreadStatic]
		private UnspawnCountBehaviourAsync _async;

		[ThreadStatic]
		private UnspawnCountBehaviourDispatch _dispatch;

		private IFuture _delayed;

		public enum EUnspawnType
		{
			Creep,
			Bot,
			Pickup
		}
	}
}
