using System;
using HeavyMetalMachines.Playback;
using Pocketverse;

namespace HeavyMetalMachines.Counselor
{
	public class CounselorParser : KeyStateParser, ICounselorDispatcher
	{
		public override StateType Type
		{
			get
			{
				return StateType.Counselor;
			}
		}

		public override void Update(BitStream stream)
		{
			int configIndex = stream.ReadCompressedInt();
			bool isActive = stream.ReadBool();
			GameHubObject.Hub.ClientCounselorController.UpdateAdvice(configIndex, isActive);
		}

		public void Send(byte targetPlayerAddress, int configIndex, bool isActive)
		{
			BitStream stream = base.GetStream();
			stream.ResetBitsWritten();
			stream.WriteCompressedInt(configIndex);
			stream.WriteBool(isActive);
			this._serverDispatcher.SendSnapshot(targetPlayerAddress, this.Type.Convert(), this._serverDispatcher.GetNextFrameId(), -1, this._gameTime.GetPlaybackTime(), stream.RawBuffer);
		}
	}
}
