using System;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[RequireComponent(typeof(Rigidbody))]
	public class PerkLinkedMovement : BasePerk, IPerkMovement
	{
		private Identifiable TargetIdentifiable
		{
			get
			{
				return (this.RefTarget != BasePerk.PerkTarget.Owner) ? this.Effect.Target : this.Effect.Owner;
			}
		}

		public override void PerkInitialized()
		{
			Transform target = this.GetTarget();
			this._body = base.gameObject.GetComponent<Rigidbody>();
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				this._attachableMath.Setup(base._trans, target, this.Type, this.Offset, this.ClientPositionModifier);
			}
			if (GameHubBehaviour.Hub.Net.IsServer() && this.TargetIdentifiable)
			{
				if (this._joint == null)
				{
					this._joint = this.TargetIdentifiable.gameObject.AddComponent<FixedJoint>();
				}
				this._body.mass = 0f;
				this._body.angularDrag = 0f;
				this._body.drag = 0f;
				this._joint.connectedBody = this._body;
				if (this._attachTargetToEffect)
				{
					CombatObject attached = CombatRef.GetCombat(target) ?? CombatRef.GetCombat(this.TargetIdentifiable.transform);
					this.Effect.Attached = attached;
				}
			}
		}

		private void LateUpdate()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				this.UpdatePosition();
			}
		}

		public Vector3 UpdatePosition()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				this._attachableMath.UpdateTransform();
			}
			return this._body.position;
		}

		protected Transform GetTarget()
		{
			Identifiable targetIdentifiable = this.TargetIdentifiable;
			if (targetIdentifiable == null)
			{
				return null;
			}
			CDummy bitComponentInChildren = targetIdentifiable.GetBitComponentInChildren<CDummy>();
			if (bitComponentInChildren == null)
			{
				return targetIdentifiable.transform;
			}
			Transform dummy = bitComponentInChildren.GetDummy(this.dummyKind, this.customDummyName);
			if (dummy == null)
			{
				return targetIdentifiable.transform;
			}
			return dummy;
		}

		public override void PerkDestroyed(DestroyEffect destroyEffect)
		{
			base.PerkDestroyed(destroyEffect);
			if (this._joint)
			{
				UnityEngine.Object.Destroy(this._joint);
			}
			this._joint = null;
			this.Effect.Attached = null;
		}

		public BasePerk.PerkTarget RefTarget;

		public CDummy.DummyKind dummyKind;

		public string customDummyName;

		public AttachableMath.ConstraintType Type;

		public Vector3 Offset;

		public Vector3 ClientPositionModifier = Vector3.one;

		[SerializeField]
		private bool _attachTargetToEffect;

		private FixedJoint _joint;

		private Rigidbody _body;

		private readonly AttachableMath _attachableMath = new AttachableMath();
	}
}
