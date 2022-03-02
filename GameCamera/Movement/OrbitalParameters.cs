using System;
using Hoplon.Input;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.GameCamera.Movement
{
	[Serializable]
	public class OrbitalParameters
	{
		[Inject]
		public void ApplyConfig(IConfigLoader config)
		{
			this.ZoomSpeed = config.GetFloatValue(ConfigAccess.OrbitalCameraZoomSpeed, this.ZoomSpeed);
			this.TurnSpeed = config.GetFloatValue(ConfigAccess.OrbitalCameraRotationSpeed, this.TurnSpeed);
			this.UnlockSpin = config.GetBoolValue(ConfigAccess.OrbitalCameraUnlockSpin, this.UnlockSpin);
		}

		public float Fov = 45f;

		public float NearPlane = 0.1f;

		public float FarPlane = 4000f;

		public float FogStart = 70f;

		public float FogEnd = 80f;

		public float MinDistance = 60f;

		public float MaxDistance = 120f;

		public float MinAngle = 15f;

		public float MaxAngle = 70f;

		public float ZoomSpeed;

		public float TurnSpeed;

		public float ZoomAcceleration;

		public float TurnAcceleration;

		public bool UnlockSpin;

		public KeyboardMouseCode ZoomIn;

		public KeyboardMouseCode ZoomOut;

		public KeyboardMouseCode TurnLeft;

		public KeyboardMouseCode TurnRight;

		public KeyboardMouseCode TurnUp;

		public KeyboardMouseCode TurnDown;

		public KeyboardMouseCode ChangeLock;
	}
}
