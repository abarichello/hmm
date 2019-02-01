using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class PickupUnspawnCountBehaviourStub : BaseComponentStub
	{
		public PickupUnspawnCountBehaviourStub(int guid) : base(guid)
		{
		}

		public IPickupUnspawnCountBehaviourAsync Async()
		{
			return this.Async(0);
		}

		public IPickupUnspawnCountBehaviourAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new PickupUnspawnCountBehaviourAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IPickupUnspawnCountBehaviourDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PickupUnspawnCountBehaviourDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IPickupUnspawnCountBehaviourDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PickupUnspawnCountBehaviourDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private PickupUnspawnCountBehaviourAsync _async;

		[ThreadStatic]
		private PickupUnspawnCountBehaviourDispatch _dispatch;
	}
}
