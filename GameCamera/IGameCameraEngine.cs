using System;
using UnityEngine;

namespace HeavyMetalMachines.GameCamera
{
	public interface IGameCameraEngine
	{
		Camera UnityCamera { get; }

		Transform CameraTransform { get; }

		Transform CurrentTargetTransform { get; }

		Vector3 CurrentTargetPosition { get; }

		void Enable();

		void Disable();

		bool IsEnabled();
	}
}
