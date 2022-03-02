using System;

namespace HeavyMetalMachines.AI.Steering
{
	public interface ISteeringBehaviour
	{
		void Evaluate(ISteeringBotContext steeringBot, ISteeringContextResult result);

		void ApplyLastEvaluation(ISteeringBotContext steeringBot, ISteeringContextResult result);

		void Format(ISteeringContextFormater formater);
	}
}
