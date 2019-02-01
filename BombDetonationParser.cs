using System;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class BombDetonationParser : KeyFrameParser
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
			TeamKind teamKind = (TeamKind)stream.ReadCompressedInt();
			int pickupInstanceId = stream.ReadCompressedInt();
			int deliveryScore = (teamKind != TeamKind.Red) ? GameHubObject.Hub.BombManager.ScoreBoard.BombScoreRed : GameHubObject.Hub.BombManager.ScoreBoard.BombScoreBlue;
			GameHubObject.Hub.BombManager.DetonateBomb(teamKind, pickupInstanceId, deliveryScore);
		}

		public void Send(TeamKind damagedTeam, int pickupId)
		{
			BitStream stream = base.GetStream();
			stream.WriteCompressedInt((int)damagedTeam);
			stream.WriteCompressedInt(pickupId);
			this.LastFrameId = PlaybackManager.BombInstance.LastId;
			this.SendKeyframe(stream.ToArray());
		}
	}
}
