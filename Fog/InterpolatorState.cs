using System;
using UnityEngine;

namespace HeavyMetalMachines.Fog
{
	[Serializable]
	public struct InterpolatorState
	{
		public float Time;

		public Vector3 Pos;

		public Vector3 Tangent;

		public Quaternion Rot;

		public Vector3 Speed;

		public float AngSpeed;

		public float TargetV;

		public bool ShouldInterpolate;

		public Vector3 Scale;
	}
}
