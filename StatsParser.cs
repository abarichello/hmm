using System;
using HeavyMetalMachines.Bank;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class StatsParser : KeyStateParser, IPlayerStatsFeature, IStatsDispatcher
	{
		public override StateType Type
		{
			get
			{
				return StateType.PlayerStats;
			}
		}

		public int BlueTeamDeaths { get; set; }

		public int RedTeamDeaths { get; set; }

		public IPlayerStatsSerialData GetStats(int objectId)
		{
			return GameHubObject.Hub.Stream.StatsStream.GetObject(objectId);
		}

		public override void Update(BitStream stream)
		{
			GameHubObject.Hub.Stream.StatsStream.ReadStream(stream);
			this.BlueTeamDeaths = stream.ReadCompressedInt();
			this.RedTeamDeaths = stream.ReadCompressedInt();
		}

		public void SendUpdate()
		{
			BitStream stream = base.GetStream();
			if (GameHubObject.Hub.Stream.StatsStream.FillSendStream(stream))
			{
				stream.WriteCompressedInt(this.BlueTeamDeaths);
				stream.WriteCompressedInt(this.RedTeamDeaths);
				base.SendUpdate(stream.ToArray());
			}
		}

		public void SendFullUpdate(byte address)
		{
			BitStream stream = base.GetStream();
			GameHubObject.Hub.Stream.StatsStream.FillSendStreamFull(stream);
			stream.WriteCompressedInt(this.BlueTeamDeaths);
			stream.WriteCompressedInt(this.RedTeamDeaths);
			base.SendFullUpdate(address, stream.ToArray());
		}
	}
}
