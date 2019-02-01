using System;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Event
{
	[Serializable]
	public class PingEvent : IEventContent, IBitStreamSerializable
	{
		public int EventTime { get; set; }

		public EventScopeKind GetKind()
		{
			return EventScopeKind.Ping;
		}

		public bool ShouldBuffer()
		{
			return false;
		}

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteCompressedInt(this.Owner);
			bs.WriteCompressedInt(this.PingKind);
			bs.WriteTeamKind(this.Team);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.Owner = bs.ReadCompressedInt();
			this.PingKind = bs.ReadCompressedInt();
			this.Team = bs.ReadTeamKind();
		}

		public int Owner = -1;

		public TeamKind Team;

		public int PingKind = -1;
	}
}
