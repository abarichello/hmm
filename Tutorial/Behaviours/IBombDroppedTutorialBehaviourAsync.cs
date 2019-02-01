using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public interface IBombDroppedTutorialBehaviourAsync : IAsync
	{
		IFuture ShowDialogOnClient();
	}
}
