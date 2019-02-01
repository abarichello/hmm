using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class ApplyLocalTransformToLocalPositionVFX : BaseVFX
	{
		public override int Priority
		{
			get
			{
				return -500;
			}
		}

		protected void Awake()
		{
			this._localPosition = base.transform.localPosition;
		}

		protected override void OnActivate()
		{
			if (!this._targetTransform)
			{
				return;
			}
			Matrix4x4 lhs = Matrix4x4.TRS(this._targetTransform.localPosition, Quaternion.identity, this._targetTransform.localScale);
			base.transform.localPosition = lhs * this._localPosition;
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
		}

		[SerializeField]
		private Transform _targetTransform;

		[NonSerialized]
		private Vector3 _localPosition;
	}
}
