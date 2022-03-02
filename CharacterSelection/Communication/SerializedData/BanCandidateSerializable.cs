using System;
using HeavyMetalMachines.CharacterSelection.Banning;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication.SerializedData
{
	public class BanCandidateSerializable : IBitStreamSerializable
	{
		public BanCandidate Data { get; set; }

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteGuid(this.Data.CharacterId);
			bs.WriteCompressedInt(this.Data.Team);
			bs.WriteBool(this.Data.IsBanned);
			bs.WriteCompressedInt(this.Data.Votes);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.Data = new BanCandidate();
			this.Data.CharacterId = bs.ReadGuid();
			this.Data.Team = bs.ReadCompressedInt();
			this.Data.IsBanned = bs.ReadBool();
			this.Data.Votes = bs.ReadCompressedInt();
		}
	}
}
