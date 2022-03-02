using System;
using UnityEngine;

namespace HeavyMetalMachines.GameCamera.Behaviour
{
	public interface IGameCameraBehaviour
	{
		void Enable(CameraState currentState);

		void Disable();

		bool IsActive { get; }

		Transform CurrentTargetTransform { get; }

		Vector3 CurrentTargetPosition { get; }

		CameraState Update(IGameCameraEngine engine, CameraState state);

		void Cleanup();
	}
}
