using System;
using System.Collections;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	[RemoteClass]
	[RequireComponent(typeof(Identifiable))]
	public class TeleportPlayerTutorialBehaviour : InGameTutorialBehaviourBase, IBitComponent
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			this.DispatchReliable(GameHubBehaviour.Hub.SendAll).ExecuteTaskOnClient(0, this._fadeInDuration);
		}

		[RemoteMethod]
		private void ExecuteTaskOnClient(int teleportPlayerTask, float taskDuration)
		{
			if (!GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (teleportPlayerTask != 0)
			{
				if (teleportPlayerTask == 1)
				{
					this.DispatchReliable(GameHubBehaviour.Hub.SendAll).TaskFinishedOnClient(1);
				}
			}
			else
			{
				this.DispatchReliable(GameHubBehaviour.Hub.SendAll).TaskFinishedOnClient(0);
			}
		}

		[RemoteMethod]
		private void TaskFinishedOnClient(int teleportPlayerTask)
		{
			if (!GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			if (teleportPlayerTask != 0)
			{
				if (teleportPlayerTask == 1)
				{
					this.UnlockMovement();
					this.CompleteBehaviourAndSync();
				}
			}
			else
			{
				this.MovePlayer();
				this.DispatchReliable(GameHubBehaviour.Hub.SendAll).ExecuteTaskOnClient(1, this._fadeOutDuration);
			}
		}

		private void UnlockMovement()
		{
			base.playerController.Combat.Movement.UnpauseSimulation();
		}

		private void MovePlayer()
		{
			base.StartCoroutine(this.ForcePositionDelayed());
		}

		private IEnumerator ForcePositionDelayed()
		{
			CombatObject bombCombat = this.GetBombCombat();
			if (this._forceTeleportBomb)
			{
				bombCombat.Movement.ResetImpulseAndVelocity();
			}
			CombatObject playerCombat = base.playerController.Combat;
			playerCombat.BoostGadget.Clear();
			playerCombat.CustomGadget0.Clear();
			playerCombat.CustomGadget1.Clear();
			playerCombat.CustomGadget2.Clear();
			playerCombat.OutOfCombatGadget.Clear();
			playerCombat.PassiveGadget.Clear();
			playerCombat.Controller.Clear();
			playerCombat.Feedback.ClearFeedbacks();
			playerCombat.Movement.ResetImpulseAndVelocity();
			playerCombat.Movement.Push(Vector3.zero, false, 0f, false);
			yield return new WaitForSeconds(this._forcePositionDelayedTime);
			if (this._forceTeleportBomb)
			{
				bombCombat.Movement.ForcePosition(this._bombTeleportTarget.position, false);
			}
			playerCombat.Movement.ForcePosition(this._teleportTarget.transform.position, true);
			playerCombat.Movement.transform.SetPositionAndRotation(this._teleportTarget.position, this._teleportTarget.rotation);
			playerCombat.Movement.PauseSimulation();
			yield return new WaitForSeconds(this._forcePositionDelayedTime);
			if (this._forceTeleportBomb && !GameHubBehaviour.Hub.BombManager.IsCarryingBomb(base.playerController.Combat.Id.ObjId))
			{
				playerCombat.BombGadget.Pressed = true;
			}
			yield return new WaitForSeconds(this._forcePositionDelayedTime);
			this.CompleteBehaviourAndSync();
			yield break;
		}

		private CombatObject GetBombCombat()
		{
			return (!(GameHubBehaviour.Hub.BombManager.BombMovement != null)) ? null : GameHubBehaviour.Hub.BombManager.BombMovement.GetComponent<CombatObject>();
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

		public ITeleportPlayerTutorialBehaviourAsync Async()
		{
			return this.Async(0);
		}

		public ITeleportPlayerTutorialBehaviourAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new TeleportPlayerTutorialBehaviourAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ITeleportPlayerTutorialBehaviourDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new TeleportPlayerTutorialBehaviourDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ITeleportPlayerTutorialBehaviourDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new TeleportPlayerTutorialBehaviourDispatch(this.OID);
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
			if (methodId == 2)
			{
				this.ExecuteTaskOnClient((int)args[0], (float)args[1]);
				return null;
			}
			if (methodId != 3)
			{
				throw new ScriptMethodNotFoundException(classId, (int)methodId);
			}
			this.TaskFinishedOnClient((int)args[0]);
			return null;
		}

		[Tooltip("Will teleport player following target's rotation")]
		[SerializeField]
		private Transform _teleportTarget;

		[SerializeField]
		private Transform _bombTeleportTarget;

		[SerializeField]
		private float _fadeInDuration;

		[SerializeField]
		private float _fadeOutDuration;

		[SerializeField]
		private bool _forceTeleportBomb;

		[SerializeField]
		private float _forcePositionDelayedTime = 0.4f;

		public const int StaticClassId = 1015;

		private Identifiable _identifiable;

		[ThreadStatic]
		private TeleportPlayerTutorialBehaviourAsync _async;

		[ThreadStatic]
		private TeleportPlayerTutorialBehaviourDispatch _dispatch;

		private IFuture _delayed;

		private enum TeleportPlayerTask
		{
			FadeIn,
			FadeOut
		}
	}
}
