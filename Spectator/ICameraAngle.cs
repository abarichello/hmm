using System;
using UnityEngine;

namespace HeavyMetalMachines.Spectator
{
	public interface ICameraAngle
	{
		IHotKey HotKey { get; }

		Transform CameraTransform { get; }

		SpectatorCameraGroupType SpectatorCameraGroupType { get; }
	}
}
