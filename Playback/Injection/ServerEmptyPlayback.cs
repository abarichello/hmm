using System;
using Pocketverse;

namespace HeavyMetalMachines.Playback.Injection
{
	internal class ServerEmptyPlayback : IPlayback
	{
		public void Init()
		{
			ServerEmptyPlayback.Log.ErrorFormatStackTrace("Trying to init on server", new object[0]);
		}

		public void Play()
		{
			ServerEmptyPlayback.Log.ErrorFormatStackTrace("Trying to play on server", new object[0]);
		}

		public void Stop()
		{
			ServerEmptyPlayback.Log.ErrorFormatStackTrace("Trying to stop on server", new object[0]);
		}

		public void AddStateKeyFrame(byte keyFrameType, int frameId, int previousFrameId, int time, byte[] data)
		{
			ServerEmptyPlayback.Log.ErrorFormatStackTrace("Trying to add keyframe on server", new object[0]);
		}

		public bool IsRunningReplay
		{
			get
			{
				ServerEmptyPlayback.Log.ErrorFormatStackTrace("Trying to check for replay on server", new object[0]);
				return false;
			}
		}

		public void SetBuffer(IMatchBuffer buffer)
		{
			ServerEmptyPlayback.Log.ErrorFormatStackTrace("Trying to set buffer on server", new object[0]);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ServerEmptyPlayback));
	}
}
