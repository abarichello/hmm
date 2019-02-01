using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class BombDroppedTutorialBehaviourAsync : BaseRemoteStub<BombDroppedTutorialBehaviourAsync>, IBombDroppedTutorialBehaviourAsync, IAsync
	{
		public BombDroppedTutorialBehaviourAsync(int guid) : base(guid)
		{
		}

		public IFuture ShowDialogOnClient()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1010, 4, new object[0]);
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
