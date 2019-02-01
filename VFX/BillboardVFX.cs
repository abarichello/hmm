using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[DisallowMultipleComponent]
	public class BillboardVFX : BaseVFX
	{
		protected void Awake()
		{
			base.enabled = false;
		}

		protected void LateUpdate()
		{
			this._target.rotation = this.ComputeRotation() * base.transform.localRotation;
		}

		private Quaternion ComputeRotation()
		{
			Quaternion result = Quaternion.identity;
			Vector3 normalized = (Camera.main.transform.position - this._target.position).normalized;
			BillboardVFX.Alignment mode = this._mode;
			if (mode != BillboardVFX.Alignment.Billboard)
			{
				if (mode == BillboardVFX.Alignment.XZOnly)
				{
					result = Quaternion.LookRotation(Vector3.Cross(Camera.main.transform.right, Vector3.up).normalized, Vector3.up);
				}
			}
			else
			{
				result = Quaternion.LookRotation(-normalized, Camera.main.transform.up);
			}
			return result;
		}

		protected override void OnActivate()
		{
			base.enabled = this._shouldUpdate;
			if (!this._shouldUpdate)
			{
				this.LateUpdate();
			}
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
			base.enabled = false;
		}

		protected void OnValidate()
		{
			if (this._target == base.transform)
			{
				this._target = null;
			}
		}

		[SerializeField]
		[Tooltip("Object to write rotation")]
		private Transform _target;

		[SerializeField]
		[Tooltip("Alignment to camera")]
		private BillboardVFX.Alignment _mode;

		[SerializeField]
		[Tooltip("Align position every frame")]
		private bool _shouldUpdate;

		public enum Alignment
		{
			Billboard,
			XZOnly
		}
	}
}
