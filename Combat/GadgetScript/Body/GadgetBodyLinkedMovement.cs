using System;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Utils;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	[RequireComponent(typeof(Rigidbody))]
	public class GadgetBodyLinkedMovement : MonoBehaviour, IGadgetBodyMovement
	{
		public bool Finished
		{
			get
			{
				return false;
			}
		}

		public Vector3 GetDirection()
		{
			return Vector3.zero;
		}

		public ICombatObject GetTarget()
		{
			return this._target;
		}

		public void Initialize(IGadgetBody body, IGadgetContext gadgetContext, IEventContext eventContext)
		{
			this._body = body;
			this._context = (IHMMGadgetContext)gadgetContext;
			if (this._linkWithOwner)
			{
				this._target = (CombatObject)this._context.GetCombatObject(this._context.OwnerId);
			}
			else
			{
				this._target = ((CombatObjectParameter)BaseParameter.GetParameter(this._targetParameter.ContentId)).GetValue(gadgetContext);
			}
			bool flag = this._target != null;
			if (this._context.IsClient && flag)
			{
				this._offset += this._body.Position - this._target.PhysicalObject.Position;
				this._attachableMath.Setup(base.transform, this._target.Transform, AttachableMath.ConstraintType.PositionAndRotationLikeChild, this._offset, this._clientPositionModifier);
			}
			if (this._context.IsServer && flag)
			{
				if (this._joint == null)
				{
					this._joint = this._target.Transform.gameObject.AddComponent<FixedJoint>();
				}
				this._joint.connectedBody = base.GetComponent<Rigidbody>();
			}
		}

		private void LateUpdate()
		{
			this.CheckTargetAlive();
			if (this._context.IsClient)
			{
				this._attachableMath.UpdateTransform();
			}
		}

		private void CheckTargetAlive()
		{
			if (!this._dettachOnTargetDeath || this._target == null || this._target.Data.IsAlive())
			{
				return;
			}
			if (this._context.IsClient)
			{
				this._attachableMath.Dettach();
			}
			if (this._context.IsServer)
			{
				this.DestroyJoint();
			}
			this._target = null;
		}

		public Vector3 GetPosition(float elapsedTime)
		{
			if (this._context.IsClient)
			{
				this._attachableMath.UpdateTransform();
			}
			return this._body.Position;
		}

		private void DestroyJoint()
		{
			if (this._joint)
			{
				UnityEngine.Object.Destroy(this._joint);
			}
			this._joint = null;
		}

		public void Destroy()
		{
			this.DestroyJoint();
		}

		[SerializeField]
		private bool _linkWithOwner = true;

		[SerializeField]
		private CombatObjectParameter _targetParameter;

		[SerializeField]
		private Vector3 _offset;

		[SerializeField]
		private Vector3 _clientPositionModifier = Vector3.one;

		[SerializeField]
		private bool _dettachOnTargetDeath;

		private FixedJoint _joint;

		private IGadgetBody _body;

		private ICombatObject _target;

		private IHMMGadgetContext _context;

		private readonly AttachableMath _attachableMath = new AttachableMath();
	}
}
