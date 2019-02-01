using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.InGame
{
	public interface IPickupDropperTutorialBehaviourAsync : IAsync
	{
		IFuture SetInterfaceScraps(string scrapText);
	}
}
