using System;
using HeavyMetalMachines.Combat;
using UnityEngine;

namespace HeavyMetalMachines.Car
{
	[Serializable]
	public class CarInfo : MovementInfo
	{
		public float ForwardAcceleration;

		public float BackwardAcceleration;

		public float BrakeAcceleration;

		public float TurningRadius;

		public bool GirosFlingHelper = true;

		public float DriftDifferenceForRecovery;

		public AnimationCurve DriftDrag;

		public float LateralFriction;

		public float GridYellowZone;

		public float GridGreenZone;
	}
}
