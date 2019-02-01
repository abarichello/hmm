using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	internal class ShakeCameraVFX : BaseVFX
	{
		private void Update()
		{
			if (Time.time - this.startTime < this.duration)
			{
				CarCamera.Singleton.Shake(this.shakeAmmount);
			}
		}

		protected override void OnActivate()
		{
			this.startTime = Time.time;
			if (CarCamera.Singleton)
			{
				CarCamera.Singleton.Shake(this.shakeAmmount);
			}
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
		}

		public float shakeAmmount;

		public float duration;

		private float startTime;
	}
}
