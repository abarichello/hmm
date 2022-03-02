using System;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	public class SteeringBotContext : ISteeringBotContext
	{
		public bool IsBotControlled { get; set; }

		public Vector3? DesiredDestination { get; set; }

		public bool IsCarryingBomb { get; set; }

		public ISteeringSubject BotSubject { get; set; }

		public ISteerringInput BotInput { get; set; }

		public ISteeringBotParameters BotParameters { get; set; }
	}
}
