using System;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	public interface ISteeringBotContext
	{
		bool IsBotControlled { get; set; }

		Vector3? DesiredDestination { get; set; }

		bool IsCarryingBomb { get; set; }

		ISteeringSubject BotSubject { get; set; }

		ISteerringInput BotInput { get; set; }

		ISteeringBotParameters BotParameters { get; set; }
	}
}
