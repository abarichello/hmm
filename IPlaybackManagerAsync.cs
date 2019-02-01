using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public interface IPlaybackManagerAsync : IAsync
	{
		IFuture AddKeyframe(byte keyframetype, int frameId, int previousFrameId, int time, byte[] data);

		IFuture UpdateState(byte statetype, byte[] data);
	}
}
