using System;
using Pocketverse;

namespace HeavyMetalMachines.Input.NoInputDetection.Business
{
	public class NoInputDetectedRpcStub : BaseComponentStub
	{
		public NoInputDetectedRpcStub(int guid) : base(guid)
		{
		}

		public INoInputDetectedRpcAsync Async()
		{
			return this.Async(0);
		}

		public INoInputDetectedRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new NoInputDetectedRpcAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public INoInputDetectedRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new NoInputDetectedRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public INoInputDetectedRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new NoInputDetectedRpcDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private NoInputDetectedRpcAsync _async;

		[ThreadStatic]
		private NoInputDetectedRpcDispatch _dispatch;
	}
}
