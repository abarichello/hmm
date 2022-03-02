using System;
using HeavyMetalMachines.GameCamera.Behaviour;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;

namespace HeavyMetalMachines.VFX
{
	public class CameraLookAtBombExplosionVFX : BaseVFX
	{
		protected override void OnActivate()
		{
			if (this._bombScoreCamera != null)
			{
				this._bombScoreCamera.LookAtExplosion(base.transform);
			}
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
		}

		[InjectOnClient]
		private IBombScoreCameraBehaviour _bombScoreCamera;
	}
}
