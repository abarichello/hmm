using System;
using Pocketverse;

namespace HeavyMetalMachines.BI
{
	public class PlayerTechnicalExperienceManagerStub : BaseComponentStub
	{
		public PlayerTechnicalExperienceManagerStub(int guid) : base(guid)
		{
		}

		public IPlayerTechnicalExperienceManagerAsync Async()
		{
			return this.Async(0);
		}

		public IPlayerTechnicalExperienceManagerAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new PlayerTechnicalExperienceManagerAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IPlayerTechnicalExperienceManagerDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PlayerTechnicalExperienceManagerDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IPlayerTechnicalExperienceManagerDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PlayerTechnicalExperienceManagerDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private PlayerTechnicalExperienceManagerAsync _async;

		[ThreadStatic]
		private PlayerTechnicalExperienceManagerDispatch _dispatch;
	}
}
