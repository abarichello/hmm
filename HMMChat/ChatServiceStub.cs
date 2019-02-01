using System;
using Pocketverse;

namespace HeavyMetalMachines.HMMChat
{
	public class ChatServiceStub : BaseComponentStub
	{
		public ChatServiceStub(int guid) : base(guid)
		{
		}

		public IChatServiceAsync Async()
		{
			return this.Async(0);
		}

		public IChatServiceAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new ChatServiceAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IChatServiceDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new ChatServiceDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IChatServiceDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new ChatServiceDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private ChatServiceAsync _async;

		[ThreadStatic]
		private ChatServiceDispatch _dispatch;
	}
}
