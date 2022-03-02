using System;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	public interface ISteeringContextMapEvaluator
	{
		float[] LastResult { get; }

		bool EvaluateDirection(ISteeringContextResult result, out Vector3 finalDirection);
	}
}
