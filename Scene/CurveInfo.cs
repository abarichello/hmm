using System;
using UnityEngine;

namespace HeavyMetalMachines.Scene
{
	[Serializable]
	public class CurveInfo
	{
		public bool lockedCurve;

		[SerializeField]
		public CurveInfo.StepPoint[] Steps;

		public float Duration = 1f;

		public Vector3 SourceTangent;

		public Vector3 TargetTangent;

		public Transform Target;

		public Transform Source;

		public AnimationCurve timeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public AnimationCurve followDirectionCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		public bool useDirection;

		[Serializable]
		public struct StepPoint
		{
			public Vector3 Center;

			public Vector3 Tangent;
		}
	}
}
