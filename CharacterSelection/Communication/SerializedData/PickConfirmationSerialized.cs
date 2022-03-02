using System;
using HeavyMetalMachines.CharacterSelection.Picking;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication.SerializedData
{
	[Serializable]
	public class PickConfirmationSerialized : IBitStreamSerializable
	{
		public PickConfirmation PickConfirmation { get; set; }

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteInt(this.PickConfirmation.Slot.Index);
			bs.WriteInt(this.PickConfirmation.Slot.Team);
			bs.WriteGuid(this.PickConfirmation.CharacterId);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.PickConfirmation = new PickConfirmation();
			this.PickConfirmation.Slot = new CharacterSelectionSlot(bs.ReadInt(), bs.ReadInt());
			this.PickConfirmation.CharacterId = bs.ReadGuid();
		}
	}
}
