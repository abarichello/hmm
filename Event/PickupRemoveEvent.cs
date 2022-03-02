using System;
using HeavyMetalMachines.Combat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Event
{
	[Serializable]
	public class PickupRemoveEvent : IEventContent, IBitStreamSerializable, IRemoveEvent
	{
		public int EventTime { get; set; }

		public EventScopeKind GetKind()
		{
			return EventScopeKind.Pickup;
		}

		public int TargetEventId { get; set; }

		public bool ShouldBuffer()
		{
			return false;
		}

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteCompressedInt(this.PickupId);
			bs.WriteCompressedInt((int)this.Reason);
			bs.WriteVector3(this.Position);
			bs.WriteCompressedInt(this.Causer);
			bs.WriteInt(this.TargetEventId);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.PickupId = bs.ReadCompressedInt();
			this.Reason = (SpawnReason)bs.ReadCompressedInt();
			this.Position = bs.ReadVector3();
			this.Causer = bs.ReadCompressedInt();
			this.TargetEventId = bs.ReadInt();
		}

		public int PickupId;

		public SpawnReason Reason;

		public Vector3 Position;

		public int Causer;
	}
}
