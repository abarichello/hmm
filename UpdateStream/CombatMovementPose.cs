using System;
using UnityEngine;

namespace HeavyMetalMachines.UpdateStream
{
	public struct CombatMovementPose
	{
		public Vector3 Position;

		public Quaternion Rotation;

		public Vector3 Velocity;

		public float AngularVelocity;
	}
}
