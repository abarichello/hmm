using System;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class BombDetonationParser : KeyFrameParser, IBombDetonationDispatcher
	{
		public override KeyFrameType Type
		{
			get
			{
				return KeyFrameType.BombDetonation;
			}
		}

		public override void Process(BitStream stream)
		{
			TeamKind damagedTeam = (TeamKind)stream.ReadCompressedInt();
			int pickupInstanceId = stream.ReadCompressedInt();
			GameHubObject.Hub.BombManager.DetonateBomb(damagedTeam, pickupInstanceId);
		}

		public void Send(TeamKind damagedTeam, int pickupId, int lastFrameId)
		{
			BitStream stream = base.GetStream();
			stream.WriteCompressedInt((int)damagedTeam);
			stream.WriteCompressedInt(pickupId);
			this.LastFrameId = lastFrameId;
			this.SendKeyframe(stream.ToArray());
		}
	}
}
