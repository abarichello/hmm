using System;
using System.Collections.Generic;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	[Serializable]
	public class RoundStats : IBitStreamSerializable
	{
		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteCompressedFloat(this.BombTimeRed);
			bs.WriteCompressedFloat(this.BombTimeBlue);
			bs.WriteCompressedFloat(this.BombTimeNeutral);
			bs.WriteTeamKind(this.DeliverTeam);
			bs.WriteCompressedInt(this.Deliverer);
			bs.WriteCompressedInt(this.DeliveryTime);
			foreach (KeyValuePair<int, PlayerRoundStats> keyValuePair in this.Players)
			{
				bs.WriteBool(true);
				bs.WriteCompressedInt(keyValuePair.Key);
				keyValuePair.Value.WriteToBitStream(bs);
			}
			bs.WriteBool(false);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.BombTimeRed = bs.ReadCompressedFloat();
			this.BombTimeBlue = bs.ReadCompressedFloat();
			this.BombTimeNeutral = bs.ReadCompressedFloat();
			this.DeliverTeam = bs.ReadTeamKind();
			this.Deliverer = bs.ReadCompressedInt();
			this.DeliveryTime = bs.ReadCompressedInt();
			while (bs.ReadBool())
			{
				int num = bs.ReadCompressedInt();
				PlayerRoundStats playerRoundStats;
				if (this.Players.TryGetValue(num, out playerRoundStats))
				{
					playerRoundStats.ReadFromBitStream(bs);
				}
				else
				{
					playerRoundStats = new PlayerRoundStats();
					playerRoundStats.ReadFromBitStream(bs);
					playerRoundStats.CarId = num;
					this.Players[num] = playerRoundStats;
				}
			}
		}

		public float BombTimeRed;

		public float BombTimeBlue;

		public float BombTimeNeutral;

		public TeamKind DeliverTeam;

		public int Deliverer;

		public int DeliveryTime;

		public Dictionary<int, PlayerRoundStats> Players = new Dictionary<int, PlayerRoundStats>();
	}
}
