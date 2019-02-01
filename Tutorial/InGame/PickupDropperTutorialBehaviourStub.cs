using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.InGame
{
	public class PickupDropperTutorialBehaviourStub : BaseComponentStub
	{
		public PickupDropperTutorialBehaviourStub(int guid) : base(guid)
		{
		}

		public IPickupDropperTutorialBehaviourAsync Async()
		{
			return this.Async(0);
		}

		public IPickupDropperTutorialBehaviourAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new PickupDropperTutorialBehaviourAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IPickupDropperTutorialBehaviourDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PickupDropperTutorialBehaviourDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IPickupDropperTutorialBehaviourDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PickupDropperTutorialBehaviourDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private PickupDropperTutorialBehaviourAsync _async;

		[ThreadStatic]
		private PickupDropperTutorialBehaviourDispatch _dispatch;
	}
}
