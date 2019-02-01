using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[RequireComponent(typeof(Rigidbody))]
	public class PerkAttachToObject : BasePerk
	{
		protected Identifiable TargetIdentifiable
		{
			get
			{
				return (this.Target != BasePerk.PerkTarget.Owner) ? this.Effect.Target : this.Effect.Owner;
			}
		}

		protected virtual Vector3 TargetInterpolatedPosition
		{
			get
			{
				return this._targetCombat.transform.position;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			this._myTransform = base.transform;
		}

		protected virtual Transform GetTarget()
		{
			Identifiable targetIdentifiable = this.TargetIdentifiable;
			if (targetIdentifiable == null)
			{
				return null;
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				CarGenerator componentInChildren = targetIdentifiable.GetComponentInChildren<CarGenerator>();
				if (componentInChildren && componentInChildren.bodyGO)
				{
					return componentInChildren.bodyGO.transform;
				}
			}
			return targetIdentifiable.transform;
		}

		public override void PerkInitialized()
		{
			this._targetTransform = this.GetTarget();
			if (!this._targetTransform)
			{
				PerkAttachToObject.Log.WarnFormat("Target={0} not found to attach effect={1} effect name={2}", new object[]
				{
					(this.Target != BasePerk.PerkTarget.Owner) ? this.Effect.Data.TargetId : this.Effect.Data.SourceId,
					this.Effect.EventId,
					this.Effect.name
				});
				return;
			}
			this._visible = true;
			this._myPreviousEuler = this._myTransform.eulerAngles;
			this._targetCombat = (CombatRef.GetCombat(this._targetTransform) ?? CombatRef.GetCombat(this.TargetIdentifiable.transform));
			if (GameHubBehaviour.Hub.Net.IsClient() && this._targetCombat)
			{
				this._offset = this._myTransform.position - this.TargetInterpolatedPosition;
			}
			else
			{
				this._offset = this._myTransform.position - this._targetTransform.position;
			}
			if (this.PullObjOnAttach)
			{
				this._targetTransform.position = this._myTransform.position;
			}
			if (this._targetCombat)
			{
				this._targetCombat.AddEffect(this.Effect);
				this.Effect.Attached = this._targetCombat;
				if (this.RotateTarget)
				{
					this._targetCombat.transform.eulerAngles = this._myPreviousEuler;
				}
				if (this.LockTargetMovement && this._targetCombat.Movement)
				{
					this._targetCombat.Movement.LockMovement();
				}
			}
			this.isSleeping = false;
			Mural.Post(new PerkAttachToObject.EffectAttachToTarget(this._targetTransform, this._targetCombat), this);
		}

		private void FixedUpdate()
		{
			if (this.isSleeping && !this.stayAttachedAfterDisable)
			{
				return;
			}
			if (!this._targetTransform)
			{
				this._targetTransform = this._myTransform;
				this.Effect.Attached = null;
				Mural.Post(new PerkAttachToObject.EffectAttachToTarget(this._targetTransform, null), this);
				return;
			}
		}

		private void LateUpdate()
		{
			if (GameHubBehaviour.Hub.Net.IsClient() && this._targetCombat && this._visible)
			{
				this._myTransform.position = this.TargetInterpolatedPosition;
			}
		}

		public override void PerkDestroyed(DestroyEffect destroyEffect)
		{
			base.PerkDestroyed(destroyEffect);
			this.isSleeping = true;
			if (this._targetCombat)
			{
				this._targetCombat.RemoveEffect(this.Effect);
				this.Effect.Attached = null;
				if (this.LockTargetMovement && this._targetCombat.Movement)
				{
					this._targetCombat.Movement.UnlockMovement();
				}
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PerkAttachToObject));

		public BasePerk.PerkTarget Target;

		public bool RotateTarget;

		public bool LockRotation;

		public bool LookToGadgetTarget;

		public bool UseOffset;

		public bool LockTargetMovement;

		public bool stayAttachedAfterDisable;

		[Header("If you use false, probably you should use PerkLinkedMovement")]
		public bool PullObjOnAttach;

		protected Transform _targetTransform;

		protected CombatObject _targetCombat;

		protected Vector3 _offset = Vector3.zero;

		protected Vector3 _myPreviousEuler;

		protected Transform _myTransform;

		private bool isSleeping;

		private bool _visible;

		public struct EffectAttachToTarget : Mural.IMuralMessage
		{
			public EffectAttachToTarget(Transform target, CombatObject combat)
			{
				this.Target = target;
				this.Combat = combat;
			}

			public string Message
			{
				get
				{
					return "OnAttachEffect";
				}
			}

			public const string Msg = "OnAttachEffect";

			public Transform Target;

			public CombatObject Combat;
		}

		public interface IEffectAttachListener
		{
			void OnAttachEffect(PerkAttachToObject.EffectAttachToTarget msg);
		}
	}
}
