using System;
using HeavyMetalMachines.Combat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Event
{
	[Serializable]
	public class CreepRemoveEvent : IEventContent, IBitStreamSerializable
	{
		public int EventTime { get; set; }

		public EventScopeKind GetKind()
		{
			return EventScopeKind.Creep;
		}

		public bool ShouldBuffer()
		{
			return false;
		}

		public void WriteToBitStream(Pocketverse.BitStream bs)
		{
			bs.WriteCompressedInt(this.CreepId);
			bs.WriteCompressedInt(this.CauserId);
			bs.WriteVector3(this.Location);
			bs.WriteCompressedInt((int)this.Reason);
		}

		public void ReadFromBitStream(Pocketverse.BitStream bs)
		{
			this.CreepId = bs.ReadCompressedInt();
			this.CauserId = bs.ReadCompressedInt();
			this.Location = bs.ReadVector3();
			this.Reason = (SpawnReason)bs.ReadCompressedInt();
		}

		public int CreepId;

		public int CauserId;

		public Vector3 Location;

		public SpawnReason Reason;
	}
}
