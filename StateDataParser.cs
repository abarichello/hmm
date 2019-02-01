using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class StateDataParser : KeyFrameParser
	{
		public override KeyFrameType Type
		{
			get
			{
				return KeyFrameType.CombatStates;
			}
		}

		public override void Process(BitStream stream)
		{
			GameHubObject.Hub.Stream.CombatDataStream.ReadStream(stream);
			GameHubObject.Hub.Stream.CombatAttStream.ReadStream(stream);
			GameHubObject.Hub.Stream.GadgetDataStream.ReadStream(stream);
			GameHubObject.Hub.Stream.SpawnControllerStream.ReadStream(stream);
			GameHubObject.Hub.Stream.StateStream.ReadStream(stream);
		}

		public override bool RewindProcess(IFrame frame)
		{
			return false;
		}

		public void SendData()
		{
			BitStream stream = base.GetStream();
			bool flag = false;
			flag |= GameHubObject.Hub.Stream.CombatDataStream.FillSendStream(stream);
			flag |= GameHubObject.Hub.Stream.CombatAttStream.FillSendStream(stream);
			flag |= GameHubObject.Hub.Stream.GadgetDataStream.FillSendStream(stream);
			flag |= GameHubObject.Hub.Stream.SpawnControllerStream.FillSendStream(stream);
			if (!(flag | GameHubObject.Hub.Stream.StateStream.FillSendStream(stream)))
			{
				return;
			}
			this.LastFrameId = -1;
			this.SendKeyframe(stream.ToArray());
		}

		public void SendFullData(byte address)
		{
			BitStream stream = base.GetStream();
			GameHubObject.Hub.Stream.CombatDataStream.FillSendStreamFull(stream);
			GameHubObject.Hub.Stream.CombatAttStream.FillSendStreamFull(stream);
			GameHubObject.Hub.Stream.GadgetDataStream.FillSendStreamFull(stream);
			GameHubObject.Hub.Stream.SpawnControllerStream.FillSendStreamFull(stream);
			GameHubObject.Hub.Stream.StateStream.FillSendStreamFull(stream);
			this.SendFullFrame(address, stream.ToArray());
		}
	}
}
