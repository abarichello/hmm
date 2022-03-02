using System;
using HeavyMetalMachines.Input.ControllerInput;

namespace HeavyMetalMachines.Spectator
{
	[Serializable]
	public struct SpectatorCameraGroupInputMapper
	{
		public SpectatorCameraGroupType CameraGroupType;

		public ControllerInputActions Action;
	}
}
