using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class GarageControllerAsync : BaseRemoteStub<GarageControllerAsync>, IGarageControllerAsync, IAsync
	{
		public GarageControllerAsync(int guid) : base(guid)
		{
		}

		public IFuture ServerPlayerOpenGadgetShop()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 9, new object[0]);
			return future;
		}

		public IFuture ServerPlayerCloseGadgetShop()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 10, new object[0]);
			return future;
		}

		public IFuture ServerBuyActivate(int gadgetKind)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 11, new object[]
			{
				gadgetKind
			});
			return future;
		}

		public IFuture ServerBuyUpgrade(int gadgetKind, string upgradeName)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 14, new object[]
			{
				gadgetKind,
				upgradeName
			});
			return future;
		}

		public IFuture ServerSellUpgrade(int gadgetKind, string upgradeName)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 18, new object[]
			{
				gadgetKind,
				upgradeName
			});
			return future;
		}

		public IFuture ServerSelectInstance(string instanceName)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1017, 28, new object[]
			{
				instanceName
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
