using System;
using HeavyMetalMachines.AI.Steering;
using HeavyMetalMachines.BotAI;

namespace HeavyMetalMachines.AI
{
	public interface IAIAgent
	{
		IBotAIDirectives Directives { get; }

		BotAIGoalManager GoalManager { get; }

		BotAIPathFind PathFind { get; }

		BotAIController Controller { get; }

		BotAIGoal Goals { get; }

		ISteeringBotContext BotContext { get; }

		ISteeringContext SteeringContext { get; }
	}
}
