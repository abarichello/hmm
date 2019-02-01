using System;
using Pocketverse;

namespace HeavyMetalMachines.HMMChat
{
	public class ChatServiceAsync : BaseRemoteStub<ChatServiceAsync>, IChatServiceAsync, IAsync
	{
		public ChatServiceAsync(int guid) : base(guid)
		{
		}

		public IFuture ReceiveMessage(bool group, string msg)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1024, 4, new object[]
			{
				group,
				msg
			});
			return future;
		}

		public IFuture ClientReceiveMessage(bool group, string msg, byte playeraddress)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1024, 7, new object[]
			{
				group,
				msg,
				playeraddress
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
