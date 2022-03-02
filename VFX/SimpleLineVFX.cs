using System;
using HeavyMetalMachines.Combat;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[RequireComponent(typeof(LineRenderer))]
	public class SimpleLineVFX : BaseVFX
	{
		protected virtual void Awake()
		{
			this._lineRenderer = base.GetComponent<LineRenderer>();
			this._lineRenderer.positionCount = 2;
		}

		protected override void OnActivate()
		{
			this._lineRenderer = base.GetComponent<LineRenderer>();
			this._startTransform = this.GetTransformFromKeyPoint(this._startPoint);
			this._endTransform = this.GetTransformFromKeyPoint(this._endPoint);
		}

		private void LateUpdate()
		{
			if (this.m_boIsActive)
			{
				this._lineRenderer.SetPosition(0, this._startTransform.position);
				this._lineRenderer.SetPosition(1, this._endTransform.position);
			}
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
			this._startTransform = null;
			this._endTransform = null;
		}

		private void OnDrawGizmosSelected()
		{
			LineRenderer component = base.GetComponent<LineRenderer>();
			int num = 0;
			for (int i = 1; i < component.positionCount; i++)
			{
				Gizmos.DrawLine(component.GetPosition(num), component.GetPosition(i));
				num = i;
			}
		}

		private Transform GetTransformFromKeyPoint(SimpleLineVFX.KeyPoint keyPoint)
		{
			switch (keyPoint)
			{
			case SimpleLineVFX.KeyPoint.OwnerDummy:
				return SimpleLineVFX.GetTransformFromDummy(this._targetFXInfo.Owner.gameObject, this._ownerDummyKind, this._ownerCustomDummyName);
			case SimpleLineVFX.KeyPoint.TargetDummy:
				return SimpleLineVFX.GetTransformFromDummy(this._targetFXInfo.Target.gameObject, this._targetDummyKind, this._targetCustomDummyName);
			case SimpleLineVFX.KeyPoint.Self:
				return base.transform;
			default:
				throw new NotImplementedException(string.Format("KeyPoint not supported: {0}", keyPoint));
			}
		}

		private static Transform GetTransformFromDummy(GameObject gameObject, CDummy.DummyKind dummyKind, string customDummyName)
		{
			CDummy componentInChildren = gameObject.GetComponentInChildren<CDummy>(true);
			return componentInChildren.GetDummy(dummyKind, customDummyName, null);
		}

		[SerializeField]
		private CDummy.DummyKind _ownerDummyKind;

		[SerializeField]
		private string _ownerCustomDummyName;

		[SerializeField]
		private CDummy.DummyKind _targetDummyKind;

		[SerializeField]
		private string _targetCustomDummyName;

		[SerializeField]
		private SimpleLineVFX.KeyPoint _startPoint;

		[SerializeField]
		private SimpleLineVFX.KeyPoint _endPoint;

		protected LineRenderer _lineRenderer;

		private Transform _startTransform;

		private Transform _endTransform;

		private enum KeyPoint
		{
			OwnerDummy,
			TargetDummy,
			Self
		}
	}
}
