using System;

namespace HeavyMetalMachines.GameCamera.Movement
{
	[Serializable]
	public class FixedCameraParameters
	{
		public float Fov = 60f;

		public float NearPlane = 0.1f;

		public float FarPlane = 4000f;

		public float FogStart = 70f;

		public float FogEnd = 80f;
	}
}
