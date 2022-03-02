using System;

namespace HeavyMetalMachines.AI.Steering
{
	public interface ISteeringContext : IAITask
	{
		void SetBotContext(ISteeringBotContext botContext);

		ISteering Steering { get; }

		void Format(ISteeringContextFormater formater);
	}
}
