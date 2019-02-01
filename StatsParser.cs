using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class StatsParser : KeyStateParser
	{
		public override StateType Type
		{
			get
			{
				return StateType.PlayerStats;
			}
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

		public int BlueTeamDeaths;

		public int RedTeamDeaths;
	}
}
