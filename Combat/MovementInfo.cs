using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[Serializable]
	public class MovementInfo
	{
		public float Mass = 1f;

		public float PushForce;

		public float PushReceived;

		public float SceneryBounciness;

		public float MaxAngularSpeed;

		public float MaxCenterZ = 3f;

		public float[] DepthOfMeshValidators;

		public float ValidatorTeleportOffset;

		public AnimationCurve Drag;

		public float LayerChangeOverlapRadius;
	}
}
