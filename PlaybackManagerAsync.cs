using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class PlaybackManagerAsync : BaseRemoteStub<PlaybackManagerAsync>, IPlaybackManagerAsync, IAsync
	{
		public PlaybackManagerAsync(int guid) : base(guid)
		{
		}

		public IFuture AddKeyframe(byte keyframetype, int frameId, int previousFrameId, int time, byte[] data)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1009, 8, new object[]
			{
				keyframetype,
				frameId,
				previousFrameId,
				time,
				data
			});
			return future;
		}

		public IFuture UpdateState(byte statetype, byte[] data)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1009, 9, new object[]
			{
				statetype,
				data
			});
			return future;
		}

		int IAsync.get_CallbackTimeoutMillis()
		{
			return base.CallbackTimeoutMillis;
		}

		void IAsync.set_CallbackTimeoutMillis(int value)
		{
			base.CallbackTimeoutMillis = value;
		}
	}
}
