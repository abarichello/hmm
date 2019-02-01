using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public abstract class KeyStateParser : AbstractParser
	{
		public abstract StateType Type { get; }

		public abstract void Update(BitStream data);

		protected void SendUpdate(byte[] data)
		{
			GameHubObject.Hub.PlaybackManager.SendState(this.Type, data);
		}

		protected void SendFullUpdate(byte address, byte[] data)
		{
			GameHubObject.Hub.PlaybackManager.SendFullState(address, this.Type, data);
		}
	}
}
