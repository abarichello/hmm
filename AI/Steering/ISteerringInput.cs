using System;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	public interface ISteerringInput
	{
		void SteerInput(float hor, float ver);

		void SteerInput(Vector3 position, bool forward);
	}
}
