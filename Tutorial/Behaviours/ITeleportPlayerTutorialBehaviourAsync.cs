using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public interface ITeleportPlayerTutorialBehaviourAsync : IAsync
	{
		IFuture ExecuteTaskOnClient(int teleportPlayerTask, float taskDuration);

		IFuture TaskFinishedOnClient(int teleportPlayerTask);
	}
}
