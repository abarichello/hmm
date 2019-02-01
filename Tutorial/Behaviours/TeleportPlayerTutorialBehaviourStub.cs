using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class TeleportPlayerTutorialBehaviourStub : BaseComponentStub
	{
		public TeleportPlayerTutorialBehaviourStub(int guid) : base(guid)
		{
		}

		public ITeleportPlayerTutorialBehaviourAsync Async()
		{
			return this.Async(0);
		}

		public ITeleportPlayerTutorialBehaviourAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new TeleportPlayerTutorialBehaviourAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ITeleportPlayerTutorialBehaviourDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new TeleportPlayerTutorialBehaviourDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ITeleportPlayerTutorialBehaviourDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new TeleportPlayerTutorialBehaviourDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private TeleportPlayerTutorialBehaviourAsync _async;

		[ThreadStatic]
		private TeleportPlayerTutorialBehaviourDispatch _dispatch;
	}
}
