using System;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.GameCamera.Movement
{
	[Serializable]
	public class SkyViewParameters
	{
		[Inject]
		public void ApplyConfig(IConfigLoader config)
		{
			this.SpectatorCloseZoom = config.GetFloatValue(ConfigAccess.SpectatorZoomClose);
			this.SpectatorNearZoom = config.GetFloatValue(ConfigAccess.SpectatorZoomNear);
			this.SpectatorFarZoom = config.GetFloatValue(ConfigAccess.SpectatorZoomFar);
		}

		public float CameraDistance = 65f;

		public float Fov = 30f;

		public float NearPlane = 0.1f;

		public float FarPlane = 4000f;

		public float FogStart = 70f;

		public float FogEnd = 80f;

		public LayerMask CollisionMask = 0;

		public float PreSpawnZoom = 4.4f;

		public float DefaultZoom = 2.4f;

		public float SpectatorCloseZoom = 1f;

		public float SpectatorNearZoom = 1.7f;

		public float SpectatorFarZoom = 4.2f;

		public AnimationCurve FocusCurve;

		public float SmoothDampDuration = 0.5f;

		public float InitialVelocityPct = 0.01f;

		[Range(0f, 1f)]
		public float ScreenArenaPercent = 0.7f;

		[Range(1f, 2f)]
		public float CameraAngleCompensationPanMultiplier = 1.35f;
	}
}
