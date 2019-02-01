using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class GarageControllerDispatch : BaseRemoteStub<GarageControllerDispatch>, IGarageControllerDispatch, IDispatch
	{
		public GarageControllerDispatch(int guid) : base(guid)
		{
		}

		public void ServerPlayerOpenGadgetShop()
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1017, 9, base.IsReliable, new object[0]);
		}

		public void ServerPlayerCloseGadgetShop()
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1017, 10, base.IsReliable, new object[0]);
		}

		public void ServerBuyActivate(int gadgetKind)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1017, 11, base.IsReliable, new object[]
			{
				gadgetKind
			});
		}

		public void ServerBuyUpgrade(int gadgetKind, string upgradeName)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1017, 14, base.IsReliable, new object[]
			{
				gadgetKind,
				upgradeName
			});
		}

		public void ServerSellUpgrade(int gadgetKind, string upgradeName)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1017, 18, base.IsReliable, new object[]
			{
				gadgetKind,
				upgradeName
			});
		}

		public void ServerSelectInstance(string instanceName)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1017, 28, base.IsReliable, new object[]
			{
				instanceName
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
