using System;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class ShakeCameraVFX : BaseVFX
	{
		private void Update()
		{
			if (this._gameCamera != null && Time.time - this.startTime < this.duration)
			{
				this._gameCamera.Shake(this.shakeAmmount);
			}
		}

		protected override void OnActivate()
		{
			if (this._gameCamera != null)
			{
				this.startTime = Time.time;
				this._gameCamera.Shake(this.shakeAmmount);
			}
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
		}

		[InjectOnClient]
		private IGameCamera _gameCamera;

		public float shakeAmmount;

		public float duration;

		private float startTime;
	}
}
