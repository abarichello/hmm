using System;

namespace HeavyMetalMachines.Spectator
{
	public interface ISpectatorCameraManager
	{
		void RegisterCameraAngle(ICameraAngle cameraAngle);

		void UnregisterCameraAngle(ICameraAngle cameraAngle);

		void UpdateCamera();
	}
}
