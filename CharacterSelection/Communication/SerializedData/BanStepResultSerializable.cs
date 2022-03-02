using System;
using System.Collections.Generic;
using HeavyMetalMachines.CharacterSelection.Banning;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication.SerializedData
{
	public class BanStepResultSerializable : IBitStreamSerializable
	{
		public BanStepResult Data { get; set; }

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteCompressedInt(this.Data.BanStepIndex);
			bs.WriteCompressedInt(this.Data.BanCandidates.Count);
			foreach (BanCandidate data in this.Data.BanCandidates)
			{
				bs.WriteBitSerializable<BanCandidateSerializable>(new BanCandidateSerializable
				{
					Data = data
				});
			}
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.Data = new BanStepResult();
			this.Data.BanStepIndex = bs.ReadCompressedInt();
			this.Data.BanCandidates = new List<BanCandidate>();
			int num = bs.ReadCompressedInt();
			for (int i = 0; i < num; i++)
			{
				BanCandidateSerializable banCandidateSerializable = new BanCandidateSerializable();
				bs.ReadBitSerializable<BanCandidateSerializable>(ref banCandidateSerializable);
				this.Data.BanCandidates.Add(banCandidateSerializable.Data);
			}
		}
	}
}
