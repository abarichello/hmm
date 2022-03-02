using System;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;

namespace HeavyMetalMachines
{
	public struct GadgetLevelData
	{
		public void WriteToStream(BitStream stream)
		{
			stream.WriteString(this.UpgradeName);
			stream.WriteBits(6, (int)this.Slot);
			stream.WriteBits(2, this.Level);
		}

		public void ReadFromStream(BitStream stream)
		{
			this.UpgradeName = stream.ReadString();
			this.Slot = (GadgetSlot)stream.ReadBits(6);
			this.Level = stream.ReadBits(2);
		}

		public GadgetSlot Slot;

		public int Level;

		public string UpgradeName;
	}
}
