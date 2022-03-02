using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PlayerPingAsync : BaseRemoteStub<PlayerPingAsync>, IPlayerPingAsync, IAsync
	{
		public PlayerPingAsync(int guid) : base(guid)
		{
		}

		public IFuture ServerCreatePing(int pingKind)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1077, 1, new object[]
			{
				pingKind
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
