using System;
using System.Collections.Generic;
using HeavyMetalMachines.Playback;
using HeavyMetalMachines.UpdateStream;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines
{
	public class TransformParser : KeyFrameParser, ITransformDispatcher
	{
		private int MaxUpdateMessageSize
		{
			get
			{
				return GameHubObject.Hub.Net.GetMaxUserPayloadSize() - 32;
			}
		}

		public override KeyFrameType Type
		{
			get
			{
				return KeyFrameType.TransformStates;
			}
		}

		public override void Process(BitStream stream)
		{
			while (stream.ReadBool())
			{
				int num = stream.ReadCompressedInt();
				byte[] array = stream.CachedReadByteArray();
				this._temporaryStream.ByteArray = array;
				MovementStream movementStream;
				if (GameHubObject.Hub.UpdateManager.MovementStreamsById.TryGetValue(num, out movementStream))
				{
					double offset = (double)GameHubObject.Hub.GameTime.RewindedTimeMillis / 1000.0;
					movementStream.Read(this._temporaryStream, offset);
				}
				else
				{
					TransformParser.Log.WarnFormat("Unknown stream id:{0} ", new object[]
					{
						num
					});
				}
				ByteArrayCache.Free(array);
			}
		}

		private void SendFrame(BitStream stream)
		{
			int nextFrameId = this._serverDispatcher.GetNextFrameId();
			stream.WriteBool(false);
			this._serverDispatcher.SendFrame(this.Type.Convert(), false, nextFrameId, this._lastFrameId, stream.ToArray());
			this._lastFrameId = nextFrameId;
		}

		public void SendMovementData(List<MovementStream> movementStreams)
		{
			BitStream stream = base.GetStream();
			this._lastFrameId = -1;
			bool flag = false;
			for (int i = 0; i < movementStreams.Count; i++)
			{
				MovementStream movementStream = movementStreams[i];
				if (movementStream.gameObject.activeInHierarchy)
				{
					this._temporaryStream.ResetBitsWritten();
					movementStream.Write(this._temporaryStream);
					int num = this._temporaryStream.CopyToArray(TransformParser._sendBuff);
					if (flag && stream.GetWrittenSize() + num + 5 >= this.MaxUpdateMessageSize)
					{
						this.SendFrame(stream);
						stream = base.GetStream();
					}
					flag = true;
					stream.WriteBool(true);
					stream.WriteCompressedInt(movementStream.Id.ObjId);
					stream.WriteByteArray(TransformParser._sendBuff, num);
				}
			}
			if (flag)
			{
				this.SendFrame(stream);
			}
		}

		[Inject]
		private IServerPlaybackDispatcher _serverDispatcher;

		protected static readonly BitLogger Log = new BitLogger(typeof(TransformParser));

		public const int RpcHeaderSizeInBytes = 32;

		private BitStream _temporaryStream = new BitStream();

		private static byte[] _sendBuff = new byte[256];

		private int _lastFrameId;
	}
}
