using System;
using HeavyMetalMachines.Character;

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
