using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public abstract class AbstractParser : GameHubObject
	{
		protected BitStream GetStream()
		{
			if (this._myStream == null)
			{
				this._myStream = new BitStream(1024);
			}
			this._myStream.ResetBitsWritten();
			return this._myStream;
		}

		private BitStream _myStream;
	}
}
