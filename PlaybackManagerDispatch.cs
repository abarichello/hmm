using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class PlaybackManagerDispatch : BaseRemoteStub<PlaybackManagerDispatch>, IPlaybackManagerDispatch, IDispatch
	{
		public PlaybackManagerDispatch(int guid) : base(guid)
		{
		}

		public void AddKeyframe(byte keyframetype, int frameId, int previousFrameId, int time, byte[] data)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1008, 9, base.IsReliable, new object[]
			{
				keyframetype,
				frameId,
				previousFrameId,
				time,
				data
			});
		}

		public void UpdateState(byte statetype, byte[] data)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1008, 10, base.IsReliable, new object[]
			{
				statetype,
				data
			});
		}

		int IDispatch.get_CallbackTimeoutMillis()
		{
			return base.CallbackTimeoutMillis;
		}

		void IDispatch.set_CallbackTimeoutMillis(int value)
		{
			base.CallbackTimeoutMillis = value;
		}

		bool IDispatch.get_IsReliable()
		{
			return base.IsReliable;
		}

		void IDispatch.set_IsReliable(bool value)
		{
			base.IsReliable = value;
		}
	}
}
