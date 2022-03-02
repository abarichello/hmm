using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.InGame
{
	public class PickupDropperTutorialBehaviourDispatch : BaseRemoteStub<PickupDropperTutorialBehaviourDispatch>, IPickupDropperTutorialBehaviourDispatch, IDispatch
	{
		public PickupDropperTutorialBehaviourDispatch(int guid) : base(guid)
		{
		}

		public void SetInterfaceScraps(string scrapText)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1013, 4, base.IsReliable, new object[]
			{
				scrapText
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
