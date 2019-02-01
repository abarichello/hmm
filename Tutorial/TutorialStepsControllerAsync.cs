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
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1009, 2, new object[]
			{
				stepIndex
			});
			return future;
		}

		public IFuture StepChangedOnServer(int step)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1009, 8, new object[]
			{
				step
			});
			return future;
		}

		public IFuture SyncBehaviourCompletedOnClient(int pStep, int pBehaviourIndex)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1009, 11, new object[]
			{
				pStep,
				pBehaviourIndex
			});
			return future;
		}

		public IFuture SyncBehaviourCompletedOnServer(int pStep, int pBehaviourIndex)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1009, 13, new object[]
			{
				pStep,
				pBehaviourIndex
			});
			return future;
		}

		public IFuture SetPlayerInputsActive(bool activeInput)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1009, 14, new object[]
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
