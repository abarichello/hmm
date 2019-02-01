using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public interface ITeleportPlayerTutorialBehaviourDispatch : IDispatch
	{
		void ExecuteTaskOnClient(int teleportPlayerTask, float taskDuration);

		void TaskFinishedOnClient(int teleportPlayerTask);
	}
}
