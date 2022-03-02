using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	[RemoteClass]
	[RequireComponent(typeof(Identifiable))]
	public class DamageTakenTutorialBehaviour : InGameTutorialBehaviourBase, IBitComponent
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			base.playerController.Combat.ListenToPosDamageTaken += this.OnPosDamageTaken;
		}

		private void OnPosDamageTaken(CombatObject causer, CombatObject taker, ModifierData mod, float f, int eventid)
		{
			if (!mod.Info.Effect.IsHPDamage())
			{
				return;
			}
			if (this.ShowDialogOnDamageTaken)
			{
				this.DispatchReliable(GameHubBehaviour.Hub.SendAll).ShowDialogOnClient();
			}
			if (this.CompleteOnFirstDamageTaken)
			{
				base.playerController.Combat.ListenToPosDamageTaken -= this.OnPosDamageTaken;
				if (!this.ShowDialogOnDamageTaken)
				{
					this.CompleteBehaviourAndSync();
				}
			}
		}

		protected override void OnStepCompletedOnServer()
		{
			base.playerController.Combat.ListenToPosDamageTaken -= this.OnPosDamageTaken;
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
				if (this.CompleteOnFirstDamageTaken)
				{
					this.CompleteBehaviourAndSync();
				}
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

		public IDamageTakenTutorialBehaviourAsync Async()
		{
			return this.Async(0);
		}

		public IDamageTakenTutorialBehaviourAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new DamageTakenTutorialBehaviourAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IDamageTakenTutorialBehaviourDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new DamageTakenTutorialBehaviourDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IDamageTakenTutorialBehaviourDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new DamageTakenTutorialBehaviourDispatch(this.OID);
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

		private static readonly BitLogger Log = new BitLogger(typeof(DamageTakenTutorialBehaviour));

		public bool CompleteOnFirstDamageTaken = true;

		public bool ShowDialogOnDamageTaken = true;

		public TutorialData TutorialData;

		public bool BlockPlayerInputs;

		public bool PauseClientAndServer;

		public const int StaticClassId = 1012;

		private Identifiable _identifiable;

		[ThreadStatic]
		private DamageTakenTutorialBehaviourAsync _async;

		[ThreadStatic]
		private DamageTakenTutorialBehaviourDispatch _dispatch;

		private IFuture _delayed;
	}
}
