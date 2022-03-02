using System;
using UnityEngine;

namespace HeavyMetalMachines.GameCamera
{
	public struct CameraState
	{
		public static CameraState Lerp(CameraState source, CameraState target, float value)
		{
			return new CameraState
			{
				Position = Vector3.Lerp(source.Position, target.Position, value),
				Rotation = Quaternion.Lerp(source.Rotation, target.Rotation, value),
				FarPlane = source.FarPlane * (1f - value) + target.FarPlane * value,
				NearPlane = source.NearPlane * (1f - value) + target.NearPlane * value,
				Fov = source.Fov * (1f - value) + target.Fov * value,
				StartFog = source.StartFog * (1f - value) + target.StartFog * value,
				EndFog = source.EndFog * (1f - value) + target.EndFog * value
			};
		}

		public Vector3 Position;

		public Quaternion Rotation;

		public float Fov;

		public float StartFog;

		public float EndFog;

		public float NearPlane;

		public float FarPlane;
	}
}
