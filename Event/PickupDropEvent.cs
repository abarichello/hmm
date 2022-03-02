using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Event
{
	[Serializable]
	public class PickupDropEvent : IEventContent, IBitStreamSerializable
	{
		public int EventTime { get; set; }

		public EventScopeKind GetKind()
		{
			return EventScopeKind.Pickup;
		}

		public bool ShouldBuffer()
		{
			return true;
		}

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteCompressedInt(this.Killer);
			bs.WriteCompressedInt(this.Causer);
			bs.WriteTeamKind(this.PickerTeam);
			bs.WriteVector3(this.Position);
			bs.WriteString(this.PickupAsset);
			bs.WriteBool(this.UnspawnOnLifeTimeEnd);
			bs.WriteCompressedInt((int)this.Reason);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.Killer = bs.ReadCompressedInt();
			this.Causer = bs.ReadCompressedInt();
			this.PickerTeam = bs.ReadTeamKind();
			this.Position = bs.ReadVector3();
			this.PickupAsset = bs.ReadString();
			this.UnspawnOnLifeTimeEnd = bs.ReadBool();
			this.Reason = (SpawnReason)bs.ReadCompressedInt();
		}

		public PickupDropEvent Clone()
		{
			return new PickupDropEvent
			{
				EventTime = this.EventTime,
				Killer = this.Killer,
				Causer = this.Causer,
				PickerTeam = this.PickerTeam,
				Position = this.Position,
				PickupAsset = this.PickupAsset,
				UnspawnOnLifeTimeEnd = this.UnspawnOnLifeTimeEnd,
				Reason = this.Reason
			};
		}

		public int Causer;

		public int Killer = -1;

		public TeamKind PickerTeam;

		public Vector3 Position;

		public string PickupAsset;

		public bool UnspawnOnLifeTimeEnd;

		public SpawnReason Reason;

		public int BuffInstanceId;
	}
}
