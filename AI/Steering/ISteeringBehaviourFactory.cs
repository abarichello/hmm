using System;

namespace HeavyMetalMachines.AI.Steering
{
	public interface ISteeringBehaviourFactory
	{
		ISteeringBehaviour CreateBehaviour(ISteeringBehaviourParameters parameters);
	}
}
