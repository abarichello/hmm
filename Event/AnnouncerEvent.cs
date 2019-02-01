using System;
using System.Collections.Generic;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Event
{
	[Serializable]
	public class AnnouncerEvent : IEventContent, IBitStreamSerializable
	{
		public int EventTime { get; set; }

		public EventScopeKind GetKind()
		{
			return EventScopeKind.Announcer;
		}

		public bool ShouldBuffer()
		{
			return false;
		}

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteCompressedInt((int)this.AnnouncerEventKind);
			bs.WriteCompressedInt(this.Killer);
			bs.WriteCompressedInt(this.Victim);
			bs.WriteCompressedInt((int)this.KillerTeam);
			bs.WriteCompressedInt(this.MainReward);
			bs.WriteCompressedInt(this.AssistReward);
			bs.WriteCompressedInt(this.AssistPlayers.Count);
			for (int i = 0; i < this.AssistPlayers.Count; i++)
			{
				bs.WriteCompressedInt(this.AssistPlayers[i]);
			}
			bs.WriteCompressedShort((short)this.LastKillStreak);
			bs.WriteCompressedShort((short)this.CurrentKillingSpree);
			bs.WriteCompressedShort((short)this.CurrentKillStreak);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.AnnouncerEventKind = (AnnouncerLog.AnnouncerEventKinds)bs.ReadCompressedInt();
			this.Killer = bs.ReadCompressedInt();
			this.Victim = bs.ReadCompressedInt();
			this.KillerTeam = (TeamKind)bs.ReadCompressedInt();
			this.MainReward = bs.ReadCompressedInt();
			this.AssistReward = bs.ReadCompressedInt();
			this.AssistPlayers.Clear();
			int num = bs.ReadCompressedInt();
			for (int i = 0; i < num; i++)
			{
				this.AssistPlayers.Add(bs.ReadCompressedInt());
			}
			this.LastKillStreak = (int)bs.ReadCompressedShort();
			this.CurrentKillingSpree = (int)bs.ReadCompressedShort();
			this.CurrentKillStreak = (int)bs.ReadCompressedShort();
		}

		public AnnouncerLog.AnnouncerEventKinds AnnouncerEventKind;

		public int Killer = -1;

		public int Victim = -1;

		public TeamKind KillerTeam;

		public int MainReward;

		public int AssistReward;

		public List<int> AssistPlayers = new List<int>();

		public int CurrentKillingSpree;

		public int CurrentKillStreak;

		public int LastKillStreak;
	}
}
