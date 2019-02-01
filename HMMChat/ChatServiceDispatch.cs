using System;
using Pocketverse;

namespace HeavyMetalMachines.HMMChat
{
	public class ChatServiceDispatch : BaseRemoteStub<ChatServiceDispatch>, IChatServiceDispatch, IDispatch
	{
		public ChatServiceDispatch(int guid) : base(guid)
		{
		}

		public void ReceiveMessage(bool group, string msg)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1024, 4, base.IsReliable, new object[]
			{
				group,
				msg
			});
		}

		public void ClientReceiveMessage(bool group, string msg, byte playeraddress)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1024, 7, base.IsReliable, new object[]
			{
				group,
				msg,
				playeraddress
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
