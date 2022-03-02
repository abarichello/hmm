using System;

namespace HeavyMetalMachines.GameCamera.Movement
{
	public interface ICameraMovement
	{
		CameraState Update(IGameCameraState camera, CameraState current, float deltaTime);

		void Reset();
	}
}
