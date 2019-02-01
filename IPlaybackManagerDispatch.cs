using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public interface IPlaybackManagerDispatch : IDispatch
	{
		void AddKeyframe(byte keyframetype, int frameId, int previousFrameId, int time, byte[] data);

		void UpdateState(byte statetype, byte[] data);
	}
}
