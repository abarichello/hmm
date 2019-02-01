using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial
{
	public class TutorialStepsControllerStub : BaseComponentStub
	{
		public TutorialStepsControllerStub(int guid) : base(guid)
		{
		}

		public ITutorialStepsControllerAsync Async()
		{
			return this.Async(0);
		}

		public ITutorialStepsControllerAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new TutorialStepsControllerAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ITutorialStepsControllerDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new TutorialStepsControllerDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ITutorialStepsControllerDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new TutorialStepsControllerDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private TutorialStepsControllerAsync _async;

		[ThreadStatic]
		private TutorialStepsControllerDispatch _dispatch;
	}
}
