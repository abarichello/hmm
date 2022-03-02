using System;
using HeavyMetalMachines.Matches;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication.SerializedData
{
	public class MatchClientSerializable : IBitStreamSerializable
	{
		public MatchClient Data { get; set; }

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteCompressedInt(this.Data.Team);
			bs.WriteCompressedLong(this.Data.PlayerId);
			bs.WriteCompressedInt(this.Data.BotId);
			bs.WriteBool(this.Data.IsBot);
			bs.WriteString(this.Data.PlayerName);
			bs.WriteCompressedInt(this.Data.PublisherId);
			bs.WriteString(this.Data.PublisherUserName);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			MatchTeam team = bs.ReadCompressedInt();
			long playerId = bs.ReadCompressedLong();
			int botId = bs.ReadCompressedInt();
			bool isBot = bs.ReadBool();
			string playerName = bs.ReadString();
			int publisherId = bs.ReadCompressedInt();
			string publisherUserName = bs.ReadString();
			MatchClient data = default(MatchClient);
			data.Team = team;
			data.PlayerId = playerId;
			data.BotId = botId;
			data.IsBot = isBot;
			data.PlayerName = playerName;
			data.PublisherId = publisherId;
			data.PublisherUserName = publisherUserName;
			this.Data = data;
		}
	}
}
