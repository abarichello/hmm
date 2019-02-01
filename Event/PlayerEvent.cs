using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Event
{
	[Serializable]
	public class PlayerEvent : IEventContent, IBitStreamSerializable
	{
		public int EventTime { get; set; }

		public virtual EventScopeKind GetKind()
		{
			return EventScopeKind.Player;
		}

		public bool ShouldBuffer()
		{
			return this.EventKind != PlayerEvent.Kind.Respawn;
		}

		public void WriteToBitStream(Pocketverse.BitStream bs)
		{
			bs.WriteCompressedInt(this.TargetId);
			bs.WriteCompressedInt(this.CauserId);
			bs.WriteByte(this.PlayerAddress);
			bs.WriteCompressedInt(this.SourceEventId);
			bs.WriteBits(4, (int)this.EventKind);
			bs.WriteVector3(this.Direction);
			bs.WriteVector3(this.Location);
			bs.WriteCompressedInt((int)this.Reason);
			bs.WriteCompressedInt(this.PossibleKiller);
			bs.WriteCompressedInt(this.Bounty);
		}

		public void ReadFromBitStream(Pocketverse.BitStream bs)
		{
			this.TargetId = bs.ReadCompressedInt();
			this.CauserId = bs.ReadCompressedInt();
			this.PlayerAddress = bs.ReadByte();
			this.SourceEventId = bs.ReadCompressedInt();
			this.EventKind = (PlayerEvent.Kind)bs.ReadBits(4);
			this.Direction = bs.ReadVector3();
			this.Location = bs.ReadVector3();
			this.Reason = (SpawnReason)bs.ReadCompressedInt();
			this.PossibleKiller = bs.ReadCompressedInt();
			this.Bounty = bs.ReadCompressedInt();
		}

		public int TargetId;

		public int CauserId;

		public byte PlayerAddress;

		public int SourceEventId;

		public PlayerEvent.Kind EventKind;

		public Vector3 Direction;

		public Vector3 Location;

		public SpawnReason Reason;

		public List<int> Assists;

		public int PossibleKiller;

		public int Bounty;

		public enum Kind
		{
			Create,
			Unspawn,
			Respawn,
			PreRespawn,
			Respawning,
			Death
		}
	}
}
