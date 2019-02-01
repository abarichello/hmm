using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class UnspawnCountBehaviourStub : BaseComponentStub
	{
		public UnspawnCountBehaviourStub(int guid) : base(guid)
		{
		}

		public IUnspawnCountBehaviourAsync Async()
		{
			return this.Async(0);
		}

		public IUnspawnCountBehaviourAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new UnspawnCountBehaviourAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IUnspawnCountBehaviourDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new UnspawnCountBehaviourDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IUnspawnCountBehaviourDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new UnspawnCountBehaviourDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private UnspawnCountBehaviourAsync _async;

		[ThreadStatic]
		private UnspawnCountBehaviourDispatch _dispatch;
	}
}
