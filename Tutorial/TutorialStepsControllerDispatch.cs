using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial
{
	public class TutorialStepsControllerDispatch : BaseRemoteStub<TutorialStepsControllerDispatch>, ITutorialStepsControllerDispatch, IDispatch
	{
		public TutorialStepsControllerDispatch(int guid) : base(guid)
		{
		}

		public void ForceStep(int stepIndex)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1010, 3, base.IsReliable, new object[]
			{
				stepIndex
			});
		}

		public void StepChangedOnServer(int step)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1010, 9, base.IsReliable, new object[]
			{
				step
			});
		}

		public void SyncBehaviourCompletedOnClient(int pStep, int pBehaviourIndex)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1010, 12, base.IsReliable, new object[]
			{
				pStep,
				pBehaviourIndex
			});
		}

		public void SyncBehaviourCompletedOnServer(int pStep, int pBehaviourIndex)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1010, 14, base.IsReliable, new object[]
			{
				pStep,
				pBehaviourIndex
			});
		}

		public void SetPlayerInputsActive(bool activeInput)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1010, 15, base.IsReliable, new object[]
			{
				activeInput
			});
		}

		int IDispatch.get_CallbackTimeoutMillis()
		{
			return base.CallbackTimeoutMillis;
		}

		void IDispatch.set_CallbackTimeoutMillis(int value)
		{
			base.CallbackTimeoutMillis = value;
		}

		bool IDispatch.get_IsReliable()
		{
			return base.IsReliable;
		}

		void IDispatch.set_IsReliable(bool value)
		{
			base.IsReliable = value;
		}
	}
}
