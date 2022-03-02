using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	[RemoteClass]
	[RequireComponent(typeof(Identifiable))]
	public class BombDroppedTutorialBehaviour : InGameTutorialBehaviourBase, IBitComponent
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			GameHubBehaviour.Hub.BombManager.ListenToBombDrop += this.OnBombDropped;
		}

		private void OnBombDropped(BombInstance bombinstance, SpawnReason reason, int causer)
		{
			if (reason == SpawnReason.TriggerDrop)
			{
				this.DispatchReliable(GameHubBehaviour.Hub.SendAll).ShowDialogOnClient();
			}
		}

		protected override void OnStepCompletedOnServer()
		{
			GameHubBehaviour.Hub.BombManager.ListenToBombDrop -= this.OnBombDropped;
		}

		[RemoteMethod]
		private void ShowDialogOnClient()
		{
			base.SetPlayerInputsActive(!this.BlockPlayerInputs);
			TutorialUIController.Instance.ShowDialog(this.TutorialData, new EventDelegate(new EventDelegate.Callback(this.CloseDialog)));
		}

		public void CloseDialog()
		{
			TutorialUIController.Instance.CloseTutorialDialog(delegate
			{
				this.CompleteBehaviourAndSync();
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

		public IBombDroppedTutorialBehaviourAsync Async()
		{
			return this.Async(0);
		}

		public IBombDroppedTutorialBehaviourAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new BombDroppedTutorialBehaviourAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IBombDroppedTutorialBehaviourDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new BombDroppedTutorialBehaviourDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IBombDroppedTutorialBehaviourDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new BombDroppedTutorialBehaviourDispatch(this.OID);
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
			if (methodId != 4)
			{
				throw new ScriptMethodNotFoundException(classId, (int)methodId);
			}
			this.ShowDialogOnClient();
			return null;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BombDroppedTutorialBehaviour));

		public TutorialData TutorialData;

		public bool BlockPlayerInputs;

		public bool PauseClientAndServer;

		public const int StaticClassId = 1011;

		private Identifiable _identifiable;

		[ThreadStatic]
		private BombDroppedTutorialBehaviourAsync _async;

		[ThreadStatic]
		private BombDroppedTutorialBehaviourDispatch _dispatch;

		private IFuture _delayed;
	}
}
