using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class CombatFeedbackParser : KeyFrameParser, ICombatFeedbackDispatcher
	{
		public override KeyFrameType Type
		{
			get
			{
				return KeyFrameType.CombatFeedbacks;
			}
		}

		public override void Process(BitStream stream)
		{
			GameHubObject.Hub.Stream.CombatFeedStream.ReadStream(stream);
		}

		public void SendData()
		{
			if (!GameHubObject.Hub.Stream.CombatFeedStream.HasChanges)
			{
				return;
			}
			BitStream stream = base.GetStream();
			GameHubObject.Hub.Stream.CombatFeedStream.FillSendStreamFull(stream);
			this.SendKeyframe(stream.ToArray());
		}

		public void SendFullData(byte address)
		{
			BitStream stream = base.GetStream();
			GameHubObject.Hub.Stream.CombatFeedStream.FillSendStreamFull(stream);
			this.SendFullFrame(address, stream.ToArray());
		}
	}
}
