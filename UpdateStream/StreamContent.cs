using System;
using Pocketverse;
using PocketVerse.Event;

namespace HeavyMetalMachines.UpdateStream
{
	public abstract class StreamContent : GameHubBehaviour, IStreamContent, ICachedObject
	{
		public short Version
		{
			get
			{
				return this._version;
			}
			set
			{
				this._version = value;
			}
		}

		public abstract int GetStreamData(ref byte[] buffer, bool boForceSerialization);

		public abstract void ApplyStreamData(byte[] data);

		protected BitStream GetStream()
		{
			if (this._stream == null)
			{
				this._stream = new BitStream(256);
			}
			this._stream.ResetBitsWritten();
			return this._stream;
		}

		protected BitStream GetStreamFor(byte[] data)
		{
			if (this._stream == null)
			{
				this._stream = new BitStream(data);
				return this._stream;
			}
			this._stream.ByteArray = data;
			return this._stream;
		}

		public virtual void OnSendToCache()
		{
		}

		public virtual void OnGetFromCache()
		{
			this.Version = short.MinValue;
		}

		private short _version = short.MinValue;

		public EventCallback<IStreamContent> OnChanged = new EventCallback<IStreamContent>();

		private BitStream _stream;
	}
}
