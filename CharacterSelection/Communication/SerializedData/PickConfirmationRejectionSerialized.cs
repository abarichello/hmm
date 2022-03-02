using System;
using HeavyMetalMachines.CharacterSelection.Picking;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication.SerializedData
{
	public class PickConfirmationRejectionSerialized : IBitStreamSerializable
	{
		public PickConfirmationRejectionReason Reason { get; set; }

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteInt(this.Reason);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.Reason = bs.ReadInt();
		}
	}
}
