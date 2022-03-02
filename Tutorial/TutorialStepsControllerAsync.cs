using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial
{
	public class TutorialStepsControllerAsync : BaseRemoteStub<TutorialStepsControllerAsync>, ITutorialStepsControllerAsync, IAsync
	{
		public TutorialStepsControllerAsync(int guid) : base(guid)
		{
		}

		public IFuture ForceStep(int stepIndex)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1010, 3, new object[]
			{
				stepIndex
			});
			return future;
		}

		public IFuture StepChangedOnServer(int step)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1010, 9, new object[]
			{
				step
			});
			return future;
		}

		public IFuture SyncBehaviourCompletedOnClient(int pStep, int pBehaviourIndex)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1010, 12, new object[]
			{
				pStep,
				pBehaviourIndex
			});
			return future;
		}

		public IFuture SyncBehaviourCompletedOnServer(int pStep, int pBehaviourIndex)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1010, 14, new object[]
			{
				pStep,
				pBehaviourIndex
			});
			return future;
		}

		public IFuture SetPlayerInputsActive(bool activeInput)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1010, 15, new object[]
			{
				activeInput
			});
			return future;
		}

		int IAsync.get_CallbackTimeoutMillis()
		{
			return base.CallbackTimeoutMillis;
		}

		void IAsync.set_CallbackTimeoutMillis(int value)
		{
			base.CallbackTimeoutMillis = value;
		}
	}
}
