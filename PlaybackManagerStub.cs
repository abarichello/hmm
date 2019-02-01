using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class PlaybackManagerStub : BaseComponentStub
	{
		public PlaybackManagerStub(int guid) : base(guid)
		{
		}

		public IPlaybackManagerAsync Async()
		{
			return this.Async(0);
		}

		public IPlaybackManagerAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new PlaybackManagerAsync(base.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IPlaybackManagerDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PlaybackManagerDispatch(base.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IPlaybackManagerDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PlaybackManagerDispatch(base.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		[ThreadStatic]
		private PlaybackManagerAsync _async;

		[ThreadStatic]
		private PlaybackManagerDispatch _dispatch;
	}
}
