using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PlayerPingStub : BaseComponentStub
	{
		public PlayerPingStub(int guid) : base(guid)
		{
		}

		public IPlayerPingAsync Async()
		{
			return this.Async(0);
		}

		public IPlayerPingAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new PlayerPingAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IPlayerPingDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PlayerPingDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IPlayerPingDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PlayerPingDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private PlayerPingAsync _async;

		[ThreadStatic]
		private PlayerPingDispatch _dispatch;
	}
}
