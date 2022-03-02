using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[HelpURL("https://confluence.hoplon.com/display/HMM/Rotate+VFX")]
	public class RotateVFX : BaseVFX
	{
		protected override void OnActivate()
		{
			this.onRotate = true;
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
			if (this.stopRotationOnDeactivate)
			{
				this.onRotate = false;
			}
		}

		private void LateUpdate()
		{
			if (!this.onRotate)
			{
				return;
			}
			base.transform.Rotate(this.rotationAdd);
		}

		[SerializeField]
		private Vector3 rotationAdd;

		[SerializeField]
		private bool stopRotationOnDeactivate;

		private bool onRotate;
	}
}
