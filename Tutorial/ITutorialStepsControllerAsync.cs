using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial
{
	public interface ITutorialStepsControllerAsync : IAsync
	{
		IFuture ForceStep(int stepIndex);

		IFuture StepChangedOnServer(int step);

		IFuture SyncBehaviourCompletedOnClient(int pStep, int pBehaviourIndex);

		IFuture SyncBehaviourCompletedOnServer(int pStep, int pBehaviourIndex);

		IFuture SetPlayerInputsActive(bool activeInput);
	}
}
