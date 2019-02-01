using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	[Serializable]
	public class AttachableMath
	{
		public AttachableMath()
		{
		}

		public AttachableMath(Transform myTrans, Transform targetTrans, AttachableMath.ConstraintType constraintType)
		{
			this.Setup(myTrans, targetTrans, constraintType);
		}

		public void Setup(Transform myTrans, Transform targetTrans, AttachableMath.ConstraintType constraintType)
		{
			this._myTrans = myTrans;
			this._target = targetTrans;
			this._constraintType = constraintType;
			this.DoMath();
		}

		public void Setup(Transform myTrans, Transform targetTrans, AttachableMath.ConstraintType constraintType, Vector3 offset, Vector3 positionModifier)
		{
			this._newOffset = offset;
			this._positionModifier = positionModifier;
			this.Setup(myTrans, targetTrans, constraintType);
		}

		public void Dettach()
		{
			this._target = null;
		}

		private void DoMath()
		{
			if (!this.IsValid())
			{
				return;
			}
			this._positionOffset = this._myTrans.position - this._target.position;
			this._myInverseRotation = Quaternion.Inverse(this._target.rotation);
			this._rotationOffset = this._myInverseRotation * this._myTrans.rotation;
			this.UpdateTransform();
		}

		public void UpdateTransform()
		{
			if (!this.IsValid())
			{
				return;
			}
			switch (this._constraintType)
			{
			case AttachableMath.ConstraintType.SamePositionAndRotation:
				this.SamePosition();
				this.SameRotation();
				break;
			case AttachableMath.ConstraintType.SameRotation:
				this.SameRotation();
				break;
			case AttachableMath.ConstraintType.RelativeRotation:
				this.RelativeRotation();
				break;
			case AttachableMath.ConstraintType.RelativePositionAndRotation:
				this.RelativeRotation();
				this.RelativePosition();
				break;
			case AttachableMath.ConstraintType.SamePositionRelativeRotation:
				this.SamePosition();
				this.RelativeRotation();
				break;
			case AttachableMath.ConstraintType.RelativePosition:
				this.RelativePosition();
				break;
			case AttachableMath.ConstraintType.PositionAndRotationLikeChild:
				this.PositionAndRotationLikeChild();
				break;
			default:
				Debug.LogError("ConstraintType not implemented!");
				break;
			}
		}

		private bool IsValid()
		{
			return this._target != null;
		}

		private void SamePosition()
		{
			this._myTrans.position = Vector3.Scale(this._target.position + this._target.rotation * this._newOffset, this._positionModifier);
		}

		private void SameRotation()
		{
			this._myTrans.rotation = this._target.rotation;
		}

		private void RelativeRotation()
		{
			this._myTrans.rotation = this._target.rotation * this._rotationOffset;
		}

		private void RelativePosition()
		{
			this._myTrans.position = Vector3.Scale(this._target.position + this._positionOffset, this._positionModifier);
		}

		private void PositionAndRotationLikeChild()
		{
			this.RelativeRotation();
			this._myTrans.position = Vector3.Scale(this._target.position + this._target.rotation * this._myInverseRotation * this._positionOffset, this._positionModifier);
		}

		private Transform _myTrans;

		private Transform _target;

		private AttachableMath.ConstraintType _constraintType;

		private Vector3 _positionOffset;

		private Vector3 _positionModifier;

		private Quaternion _rotationOffset;

		private Quaternion _myInverseRotation;

		private Vector3 _newOffset;

		public enum ConstraintType
		{
			SamePositionAndRotation,
			SameRotation,
			RelativeRotation,
			RelativePositionAndRotation,
			SamePositionRelativeRotation,
			RelativePosition,
			PositionAndRotationLikeChild
		}
	}
}
