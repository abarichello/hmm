using System;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	public interface ISteering
	{
		void StopMoving(ISteeringBotContext botContext);

		void SteerToPosition(ISteeringBotContext botContext, Vector3 position);

		void Update(ISteeringBotContext botContext);

		bool MovingCar { get; }

		bool AcceleratingForward { get; }

		Vector3 MousePosition { get; }
	}
}
