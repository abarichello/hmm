using System;

namespace HeavyMetalMachines.Spectator
{
	public interface ISpectatorService
	{
		bool IsSpectating { get; }

		void SetFixedCamera(ICameraAngle camera);

		CameraZoomLevel GetCurrentZoomLevel();
	}
}
