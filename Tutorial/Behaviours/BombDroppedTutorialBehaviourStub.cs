using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class BombDroppedTutorialBehaviourStub : BaseComponentStub
	{
		public BombDroppedTutorialBehaviourStub(int guid) : base(guid)
		{
		}

		public IBombDroppedTutorialBehaviourAsync Async()
		{
			return this.Async(0);
		}

		public IBombDroppedTutorialBehaviourAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new BombDroppedTutorialBehaviourAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IBombDroppedTutorialBehaviourDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new BombDroppedTutorialBehaviourDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IBombDroppedTutorialBehaviourDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new BombDroppedTutorialBehaviourDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private BombDroppedTutorialBehaviourAsync _async;

		[ThreadStatic]
		private BombDroppedTutorialBehaviourDispatch _dispatch;
	}
}
