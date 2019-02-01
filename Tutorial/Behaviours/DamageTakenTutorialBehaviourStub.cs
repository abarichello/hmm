using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class DamageTakenTutorialBehaviourStub : BaseComponentStub
	{
		public DamageTakenTutorialBehaviourStub(int guid) : base(guid)
		{
		}

		public IDamageTakenTutorialBehaviourAsync Async()
		{
			return this.Async(0);
		}

		public IDamageTakenTutorialBehaviourAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new DamageTakenTutorialBehaviourAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IDamageTakenTutorialBehaviourDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new DamageTakenTutorialBehaviourDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IDamageTakenTutorialBehaviourDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new DamageTakenTutorialBehaviourDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private DamageTakenTutorialBehaviourAsync _async;

		[ThreadStatic]
		private DamageTakenTutorialBehaviourDispatch _dispatch;
	}
}
