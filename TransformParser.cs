using System;
using System.Collections.Generic;
using HeavyMetalMachines.UpdateStream;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class TransformParser : KeyFrameParser
	{
		public TransformParser()
		{
			this._temporaryStream = new BitStream();
		}

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
				this._temporaryStream.ByteArray = stream.ReadByteArray();
				StreamObject streamObject;
				MovementStream movementStream;
				if (GameHubObject.Hub.UpdateManager.StreamObjectsMap.TryGetValue(num, out streamObject))
				{
					streamObject.StreamRead(true, this._temporaryStream, GameHubObject.Hub.GameTime.RewindedTimeMillis);
				}
				else if (GameHubObject.Hub.UpdateManager.MovementStreamsById.TryGetValue(num, out movementStream))
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
			}
			float time = stream.ReadCompressedFloat();
			int synch = stream.ReadCompressedInt();
			if (GameHubObject.Hub.playbackSystem.State != PlaybackSystem.PlaybackState.Replay)
			{
				GameHubObject.Hub.Clock.FixDelta(time, synch);
			}
		}

		public void SendData()
		{
			BitStream stream = base.GetStream();
			List<StreamObject> streamObjects = GameHubObject.Hub.UpdateManager.StreamObjects;
			this._lastFrameId = -1;
			bool flag = false;
			for (int i = 0; i < streamObjects.Count; i++)
			{
				StreamObject streamObject = streamObjects[i];
				if (streamObject.gameObject.activeInHierarchy)
				{
					int num = streamObject.StreamWrite(ref TransformParser._sendBuff);
					if (flag && stream.GetWrittenSize() + num + 5 >= this.MaxUpdateMessageSize)
					{
						this.SendFrame(stream);
						stream = base.GetStream();
					}
					flag = true;
					stream.WriteBool(true);
					stream.WriteCompressedInt(streamObject.Id.ObjId);
					stream.WriteByteArray(TransformParser._sendBuff, num);
				}
			}
			if (flag)
			{
				this.SendFrame(stream);
			}
		}

		private void SendFrame(BitStream stream)
		{
			int num = GameHubObject.Hub.PlaybackManager.NextId();
			stream.WriteBool(false);
			stream.WriteCompressedFloat(GameHubObject.Hub.GameTime.GetPlaybackUnityTime());
			stream.WriteCompressedInt(GameHubObject.Hub.GameTime.GetSynchTime());
			GameHubObject.Hub.PlaybackManager.SendKeyFrame(this.Type, false, num, this._lastFrameId, stream.ToArray());
			this._lastFrameId = num;
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

		private static readonly BitLogger Log = new BitLogger(typeof(TransformParser));

		public const int RpcHeaderSizeInBytes = 32;

		private BitStream _temporaryStream;

		private static byte[] _sendBuff = new byte[256];

		private int _lastFrameId;
	}
}
