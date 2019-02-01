using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	[Serializable]
	public class PlayerRoundStats : IBitStreamSerializable
	{
		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteCompressedFloat(this.BombTime);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.BombTime = bs.ReadCompressedFloat();
		}

		public int CarId;

		public float BombTime;
	}
}
