using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Event
{
	[Serializable]
	public class CreepEvent : IEventContent, IBitStreamSerializable
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
			bs.WriteCompressedInt(this.CreepInfoId);
			bs.WriteCompressedInt(this.Amount);
			bs.WriteCompressedInt((int)this.CreepTeam);
			bs.WriteCompressedInt((int)this.Reason);
			bs.WriteVector3(this.Direction);
			bs.WriteVector3(this.Location);
			bs.WriteCompressedShort((short)this.Level);
			bs.WriteFloat(this.BotAggroMaxDistance);
		}

		public void ReadFromBitStream(Pocketverse.BitStream bs)
		{
			this.CreepId = bs.ReadCompressedInt();
			this.CauserId = bs.ReadCompressedInt();
			this.CreepInfoId = bs.ReadCompressedInt();
			this.Amount = bs.ReadCompressedInt();
			this.CreepTeam = (TeamKind)bs.ReadCompressedInt();
			this.Reason = (SpawnReason)bs.ReadCompressedInt();
			this.Direction = bs.ReadVector3();
			this.Location = bs.ReadVector3();
			this.Level = (int)bs.ReadCompressedShort();
			this.BotAggroMaxDistance = bs.ReadFloat();
		}

		public CreepEvent SingleClone()
		{
			return new CreepEvent
			{
				EventTime = this.EventTime,
				CreepId = this.CreepId,
				CreepInfoId = this.CreepInfoId,
				CauserId = this.CauserId,
				CreepTeam = this.CreepTeam,
				Reason = this.Reason,
				Amount = 1,
				Direction = this.Direction,
				Location = this.Location,
				Level = this.Level,
				BotAggroMaxDistance = this.BotAggroMaxDistance
			};
		}

		public int CreepId;

		public int CreepInfoId;

		public int CauserId;

		public TeamKind CreepTeam;

		public SpawnReason Reason;

		public int Amount = 1;

		public Vector3 Direction;

		public Vector3 Location;

		public int Level = 1;

		public float BotAggroMaxDistance;
	}
}
