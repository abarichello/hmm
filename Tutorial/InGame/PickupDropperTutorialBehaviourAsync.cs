using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.InGame
{
	public class PickupDropperTutorialBehaviourAsync : BaseRemoteStub<PickupDropperTutorialBehaviourAsync>, IPickupDropperTutorialBehaviourAsync, IAsync
	{
		public PickupDropperTutorialBehaviourAsync(int guid) : base(guid)
		{
		}

		public IFuture SetInterfaceScraps(string scrapText)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1013, 4, new object[]
			{
				scrapText
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
