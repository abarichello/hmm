using System;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	internal interface IAvoidElementParameters
	{
		IAIElementKind ElementKind { get; }

		AnimationCurve HPToActivationCurve { get; }

		bool OnlyWhenCarryingBomb { get; }

		float DangerForce { get; }

		float InterestForce { get; }

		float Range { get; }
	}
}
