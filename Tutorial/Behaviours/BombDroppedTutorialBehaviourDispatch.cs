using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class BombDroppedTutorialBehaviourDispatch : BaseRemoteStub<BombDroppedTutorialBehaviourDispatch>, IBombDroppedTutorialBehaviourDispatch, IDispatch
	{
		public BombDroppedTutorialBehaviourDispatch(int guid) : base(guid)
		{
		}

		public void ShowDialogOnClient()
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1011, 4, base.IsReliable, new object[0]);
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
