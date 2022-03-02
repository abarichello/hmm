using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication.SerializedData
{
	public class CharacterSelectionResultSerialized : IBitStreamSerializable
	{
		public CharacterSelectionResult Data { get; set; }

		public void WriteToBitStream(BitStream bs)
		{
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.Data = new CharacterSelectionResult();
		}
	}
}
