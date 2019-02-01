using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public static class GadgetSlotEx
	{
		public static void WriteGadgetSlot(this BitStream stream, GadgetSlot slot)
		{
			stream.WriteBits(5, (int)slot);
		}

		public static GadgetSlot ReadGadgetSlot(this BitStream stream)
		{
			int num = stream.ReadBits(5);
			if (num == 31)
			{
				num = -1;
			}
			return (GadgetSlot)num;
		}
	}
}
