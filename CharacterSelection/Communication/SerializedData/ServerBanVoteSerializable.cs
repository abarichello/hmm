using System;
using HeavyMetalMachines.CharacterSelection.Banning;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication.SerializedData
{
	public class ServerBanVoteSerializable : IBitStreamSerializable
	{
		public ServerBanVoteConfirmation Data { get; set; }

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteBitSerializable<MatchClientSerializable>(new MatchClientSerializable
			{
				Data = this.Data.Client
			});
			bool flag = this.Data.CharacterId != null;
			bs.WriteBool(flag);
			if (flag)
			{
				bs.WriteGuid(this.Data.CharacterId.Value);
			}
		}

		public void ReadFromBitStream(BitStream bs)
		{
			MatchClientSerializable matchClientSerializable = new MatchClientSerializable();
			bs.ReadBitSerializable<MatchClientSerializable>(ref matchClientSerializable);
			bool flag = bs.ReadBool();
			Guid? characterId;
			if (flag)
			{
				characterId = new Guid?(bs.ReadGuid());
			}
			else
			{
				characterId = null;
			}
			ServerBanVoteConfirmation data = default(ServerBanVoteConfirmation);
			data.CharacterId = characterId;
			data.Client = matchClientSerializable.Data;
			this.Data = data;
		}
	}
}
