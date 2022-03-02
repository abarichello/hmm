using System;
using HeavyMetalMachines.AI.Steering;
using HeavyMetalMachines.BotAI;

namespace HeavyMetalMachines.AI
{
	public class HMMAgent : IAIAgent
	{
		public IBotAIDirectives Directives
		{
			get
			{
				return this.GoalManager;
			}
		}

		public BotAIGoalManager GoalManager { get; set; }

		public BotAIPathFind PathFind { get; set; }

		public BotAIController Controller { get; set; }

		public BotAIGoal Goals { get; set; }

		public ISteeringBotContext BotContext { get; set; }

		public ISteeringContext SteeringContext { get; set; }
	}
}
