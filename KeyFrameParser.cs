using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public abstract class KeyFrameParser : AbstractParser, IKeyFrameParser
	{
		public abstract KeyFrameType Type { get; }

		public abstract void Process(BitStream stream);

		public virtual bool RewindProcess(IFrame frame)
		{
			return true;
		}

		protected virtual void SendKeyframe(byte[] data)
		{
			int num = GameHubObject.Hub.PlaybackManager.NextId();
			GameHubObject.Hub.PlaybackManager.SendKeyFrame(this.Type, true, num, this.LastFrameId, data);
			this.LastFrameId = num;
		}

		protected virtual void SendFullFrame(byte address, byte[] data)
		{
			GameHubObject.Hub.PlaybackManager.SendFullKeyFrame(address, this.Type, GameHubObject.Hub.PlaybackManager.NextId(), -1, GameHubObject.Hub.GameTime.GetPlaybackTime(), data);
		}

		protected int LastFrameId = -1;
	}
}
