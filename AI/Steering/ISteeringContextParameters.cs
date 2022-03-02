using System;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	public interface ISteeringContextParameters
	{
		int DirectionCount { get; }

		float DegreesPerDirection { get; }

		int GetOppositeDirectionIndex(int index);

		Vector3 GetDirection(int index);

		float RoundToIndex(Vector3 direction);

		int GetMirroredDirection(int index, float axisIndex);
	}
}
