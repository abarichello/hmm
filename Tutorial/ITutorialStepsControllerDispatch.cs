using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial
{
	public interface ITutorialStepsControllerDispatch : IDispatch
	{
		void ForceStep(int stepIndex);

		void StepChangedOnServer(int step);

		void SyncBehaviourCompletedOnClient(int pStep, int pBehaviourIndex);

		void SyncBehaviourCompletedOnServer(int pStep, int pBehaviourIndex);

		void SetPlayerInputsActive(bool activeInput);
	}
}
