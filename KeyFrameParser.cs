using System;
using HeavyMetalMachines.Playback;
using Pocketverse;
using Zenject;

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
			int nextFrameId = this._dispatcher.GetNextFrameId();
			this._dispatcher.SendFrame(this.Type.Convert(), true, nextFrameId, this.LastFrameId, data);
			this.LastFrameId = nextFrameId;
		}

		protected virtual void SendFullFrame(byte address, byte[] data)
		{
			int nextFrameId = this._dispatcher.GetNextFrameId();
			int playbackTime = this._gameTime.GetPlaybackTime();
			this._dispatcher.SendSnapshot(address, this.Type.Convert(), nextFrameId, -1, playbackTime, data);
		}

		protected int LastFrameId = -1;

		[Inject]
		protected IServerPlaybackDispatcher _dispatcher;

		[Inject]
		protected IGameTime _gameTime;
	}
}
