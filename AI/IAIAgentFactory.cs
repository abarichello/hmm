using System;
using UnityEngine;

namespace HeavyMetalMachines.AI
{
	public interface IAIAgentFactory
	{
		IAIAgent CreateAIAgent(GameObject agentObject);
	}
}
