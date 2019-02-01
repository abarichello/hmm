using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public interface IDamageTakenTutorialBehaviourDispatch : IDispatch
	{
		void ShowDialogOnClient();
	}
}
