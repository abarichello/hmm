using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class InputManagerStub : BaseComponentStub
	{
		public InputManagerStub(int guid) : base(guid)
		{
		}

		public IInputManagerAsync Async()
		{
			return this.Async(0);
		}

		public IInputManagerAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new InputManagerAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IInputManagerDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new InputManagerDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IInputManagerDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new InputManagerDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private InputManagerAsync _async;

		[ThreadStatic]
		private InputManagerDispatch _dispatch;
	}
}
