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
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1025, 5, new object[]
			{
				group,
				msg
			});
			return future;
		}

		public IFuture ReceiveDraftMessage(bool toTeam, string draft, string context, string[] messageParameters)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1025, 6, new object[]
			{
				toTeam,
				draft,
				context,
				messageParameters
			});
			return future;
		}

		public IFuture ClientReceiveMessage(bool group, string msg, byte playeraddress)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1025, 10, new object[]
			{
				group,
				msg,
				playeraddress
			});
			return future;
		}

		public IFuture ClientReceiveDraftMessage(bool toTeam, string draft, string context, string[] messageParameters, byte playeraddress)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1025, 11, new object[]
			{
				toTeam,
				draft,
				context,
				messageParameters,
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
