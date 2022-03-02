using System;

namespace HeavyMetalMachines.GameCamera.Movement
{
	public class FixedCameraMovement : ICameraMovement
	{
		public FixedCameraMovement(FixedCameraParameters parameters)
		{
			this._parameters = parameters;
		}

		public void Reset()
		{
		}

		public CameraState Update(IGameCameraState camera, CameraState current, float deltaTime)
		{
			return new CameraState
			{
				NearPlane = this._parameters.NearPlane,
				FarPlane = this._parameters.FarPlane,
				Fov = this._parameters.Fov,
				StartFog = this._parameters.FogStart,
				EndFog = this._parameters.FogEnd,
				Position = camera.CurrentTargetTransform.position,
				Rotation = camera.CurrentTargetTransform.rotation
			};
		}

		private readonly FixedCameraParameters _parameters;
	}
}
