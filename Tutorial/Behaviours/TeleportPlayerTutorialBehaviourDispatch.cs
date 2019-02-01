using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class TeleportPlayerTutorialBehaviourDispatch : BaseRemoteStub<TeleportPlayerTutorialBehaviourDispatch>, ITeleportPlayerTutorialBehaviourDispatch, IDispatch
	{
		public TeleportPlayerTutorialBehaviourDispatch(int guid) : base(guid)
		{
		}

		public void ExecuteTaskOnClient(int teleportPlayerTask, float taskDuration)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1014, 2, base.IsReliable, new object[]
			{
				teleportPlayerTask,
				taskDuration
			});
		}

		public void TaskFinishedOnClient(int teleportPlayerTask)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1014, 3, base.IsReliable, new object[]
			{
				teleportPlayerTask
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
