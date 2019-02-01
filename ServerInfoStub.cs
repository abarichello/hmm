using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class ServerInfoStub : BaseComponentStub
	{
		public ServerInfoStub(int guid) : base(guid)
		{
		}

		public IServerInfoAsync Async()
		{
			return this.Async(0);
		}

		public IServerInfoAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new ServerInfoAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IServerInfoDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new ServerInfoDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IServerInfoDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new ServerInfoDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private ServerInfoAsync _async;

		[ThreadStatic]
		private ServerInfoDispatch _dispatch;
	}
}
