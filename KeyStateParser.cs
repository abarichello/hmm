using System;
using HeavyMetalMachines.Playback;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines
{
	public abstract class KeyStateParser : AbstractParser, IKeyStateParser
	{
		public abstract StateType Type { get; }

		public abstract void Update(BitStream data);

		protected void SendUpdate(byte[] data)
		{
			this._serverDispatcher.SendFrame(this.Type.Convert(), true, this._serverDispatcher.GetNextFrameId(), -1, data);
		}

		protected void SendFullUpdate(byte address, byte[] data)
		{
			this._serverDispatcher.SendSnapshot(address, this.Type.Convert(), this._serverDispatcher.GetNextFrameId(), -1, this._gameTime.GetPlaybackTime(), data);
		}

		[Inject]
		protected IServerPlaybackDispatcher _serverDispatcher;

		[Inject]
		protected IGameTime _gameTime;
	}
}
