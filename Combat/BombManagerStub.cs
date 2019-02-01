using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class BombManagerStub : BaseComponentStub
	{
		public BombManagerStub(int guid) : base(guid)
		{
		}

		public IBombManagerAsync Async()
		{
			return this.Async(0);
		}

		public IBombManagerAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new BombManagerAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IBombManagerDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new BombManagerDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IBombManagerDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new BombManagerDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private BombManagerAsync _async;

		[ThreadStatic]
		private BombManagerDispatch _dispatch;
	}
}
