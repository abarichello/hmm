using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.BotAI
{
	public class BotAIHub : GameHubBehaviour
	{
		public GarageController GarageControllerRed;

		public GarageController GarageControllerBlu;

		public List<Transform> RepairPoints = new List<Transform>();
	}
}
