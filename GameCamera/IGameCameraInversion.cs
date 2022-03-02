using System;
using HeavyMetalMachines.Arena;
using UnityEngine;

namespace HeavyMetalMachines.GameCamera
{
	public interface IGameCameraInversion
	{
		float ScreenSpaceAngle { get; }

		Vector3 InversionAngle { get; }

		bool CameraInverted { get; set; }

		void SetupArena(IGameArenaInfo config);
	}
}
