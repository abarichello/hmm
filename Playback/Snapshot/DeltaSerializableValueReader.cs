using System;
using Pocketverse;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public static class DeltaSerializableValueReader
	{
		public static void ReadIntField(ref int field, BitStream stream)
		{
			if (stream.ReadBool())
			{
				field = stream.ReadCompressedInt();
			}
		}

		public static void ReadFloatField(ref float field, BitStream stream)
		{
			if (stream.ReadBool())
			{
				field = stream.ReadFloat();
			}
		}

		public static void ReadFloatPrecisionField(ref float field, int precision, BitStream stream)
		{
			if (stream.ReadBool())
			{
				field = stream.ReadCompressedFixedPoint(precision);
			}
		}
	}
}
