using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class TeleportPlayerTutorialBehaviourAsync : BaseRemoteStub<TeleportPlayerTutorialBehaviourAsync>, ITeleportPlayerTutorialBehaviourAsync, IAsync
	{
		public TeleportPlayerTutorialBehaviourAsync(int guid) : base(guid)
		{
		}

		public IFuture ExecuteTaskOnClient(int teleportPlayerTask, float taskDuration)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1015, 2, new object[]
			{
				teleportPlayerTask,
				taskDuration
			});
			return future;
		}

		public IFuture TaskFinishedOnClient(int teleportPlayerTask)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1015, 3, new object[]
			{
				teleportPlayerTask
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
