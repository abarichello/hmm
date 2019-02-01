using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class PauseControllerStub : BaseComponentStub
	{
		public PauseControllerStub(int guid) : base(guid)
		{
		}

		public IPauseControllerAsync Async()
		{
			return this.Async(0);
		}

		public IPauseControllerAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new PauseControllerAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IPauseControllerDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PauseControllerDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IPauseControllerDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PauseControllerDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private PauseControllerAsync _async;

		[ThreadStatic]
		private PauseControllerDispatch _dispatch;
	}
}
