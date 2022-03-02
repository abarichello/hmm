using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication.SerializedData
{
	public class CharacterSelectionSlotSerialized : IBitStreamSerializable
	{
		public CharacterSelectionSlot Slot { get; set; }

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteInt(this.Slot.Index);
			bs.WriteInt(this.Slot.Team);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.Slot = new CharacterSelectionSlot(bs.ReadInt(), bs.ReadInt());
		}
	}
}
