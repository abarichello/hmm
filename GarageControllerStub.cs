using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class GarageControllerStub : BaseComponentStub
	{
		public GarageControllerStub(int guid) : base(guid)
		{
		}

		public IGarageControllerAsync Async()
		{
			return this.Async(0);
		}

		public IGarageControllerAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new GarageControllerAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IGarageControllerDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new GarageControllerDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IGarageControllerDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new GarageControllerDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private GarageControllerAsync _async;

		[ThreadStatic]
		private GarageControllerDispatch _dispatch;
	}
}
