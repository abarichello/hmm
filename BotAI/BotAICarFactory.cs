using System;
using HeavyMetalMachines.Characters;

namespace HeavyMetalMachines.BotAI
{
	public class BotAICarFactory : PlayerCarFactory
	{
		private void Awake()
		{
			this.IsBot = true;
		}
	}
}
