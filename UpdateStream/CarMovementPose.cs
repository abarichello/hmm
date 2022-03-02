using System;
using UnityEngine;

namespace HeavyMetalMachines.UpdateStream
{
	public struct CarMovementPose
	{
		public Vector3 Position;

		public Quaternion Rotation;

		public Vector3 Velocity;

		public float AngularVelocity;

		public float TargetH;

		public float TargetV;

		public float HAxis;

		public float VAxis;

		public float SpeedZ;

		public bool IsDrifting;

		public float TurretAngle;
	}
}
