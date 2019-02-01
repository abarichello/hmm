using System;
using Pocketverse;

namespace HeavyMetalMachines.BI
{
	public struct ExperienceDataSet : IBitStreamSerializable
	{
		public static ExperienceDataSet operator +(ExperienceDataSet one, ExperienceDataSet other)
		{
			ExperienceDataSet result;
			result.FreezeAcc = one.FreezeAcc + other.FreezeAcc;
			result.FreezeCount = one.FreezeCount + other.FreezeCount;
			return result;
		}

		public static bool operator >(ExperienceDataSet one, ExperienceDataSet other)
		{
			return one.FreezeAcc > other.FreezeAcc || one.FreezeCount > other.FreezeCount;
		}

		public static bool operator <(ExperienceDataSet one, ExperienceDataSet other)
		{
			return !(one > other);
		}

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteLong(this.FreezeAcc);
			bs.WriteCompressedInt(this.FreezeCount);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.FreezeAcc = bs.ReadLong();
			this.FreezeCount = bs.ReadCompressedInt();
		}

		public long FreezeAcc;

		public int FreezeCount;
	}
}
