using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication.SerializedData
{
	public class CharacterChoiceSerialized : IBitStreamSerializable
	{
		public CharacterChoice CharacterChoice { get; set; }

		public void WriteToBitStream(BitStream bs)
		{
			CharacterSelectionSlotSerialized value = new CharacterSelectionSlotSerialized
			{
				Slot = this.CharacterChoice.Slot
			};
			bs.WriteBitSerializable<CharacterSelectionSlotSerialized>(value);
			bs.WriteGuid(this.CharacterChoice.CharacterId);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			CharacterSelectionSlotSerialized characterSelectionSlotSerialized = new CharacterSelectionSlotSerialized();
			bs.ReadBitSerializable<CharacterSelectionSlotSerialized>(ref characterSelectionSlotSerialized);
			Guid characterId = bs.ReadGuid();
			this.CharacterChoice = new CharacterChoice
			{
				Slot = characterSelectionSlotSerialized.Slot,
				CharacterId = characterId
			};
		}
	}
}
