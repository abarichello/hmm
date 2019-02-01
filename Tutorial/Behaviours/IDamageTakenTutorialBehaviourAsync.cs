using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public interface IDamageTakenTutorialBehaviourAsync : IAsync
	{
		IFuture ShowDialogOnClient();
	}
}
