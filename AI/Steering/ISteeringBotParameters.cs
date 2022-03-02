using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.AI.Steering
{
	public interface ISteeringBotParameters
	{
		float DirectionalSteeringSnapMultiplier { get; }

		IList<ISteeringBehaviourParameters> SteeringBehaviours { get; }
	}
}
