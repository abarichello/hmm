using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PlayerPingDispatch : BaseRemoteStub<PlayerPingDispatch>, IPlayerPingDispatch, IDispatch
	{
		public PlayerPingDispatch(int guid) : base(guid)
		{
		}

		public void ServerCreatePing(int pingKind)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1077, 1, base.IsReliable, new object[]
			{
				pingKind
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
