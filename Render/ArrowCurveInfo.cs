using System;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	[Serializable]
	public class ArrowCurveInfo
	{
		public Vector3 lastEvaluation;

		public float Duration = 1f;

		public Vector3 SourceTangent;

		public Vector3 TargetTangent;

		public Transform Target;

		public Transform Source;

		public Vector3 RelativePosition;

		public Vector3 InitialPosition;

		public bool useDirection;

		[Serializable]
		public struct StepPoint
		{
			public Vector3 Center;

			public Vector3 Tangent;
		}
	}
}
