using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class ScaleGadgetRadiusVFX : BaseVFX
	{
		private void Awake()
		{
			base.enabled = false;
		}

		protected override void OnActivate()
		{
			if (this.Target)
			{
				this.Target.SetActive(true);
			}
			this.Target.transform.localScale = new Vector3(this._targetFXInfo.Gadget.Radius, this._targetFXInfo.Gadget.Radius, this._targetFXInfo.Gadget.Radius);
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
			if (this.Target)
			{
				this.Target.SetActive(false);
			}
			base.enabled = false;
		}

		private Vector3 _targetScale;

		private float _lifeTime;

		private float _t;

		public GameObject Target;
	}
}
