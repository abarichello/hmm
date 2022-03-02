using System;
using HeavyMetalMachines.Combat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Event
{
	[Serializable]
	public class EffectRemoveEvent : IEventContent, IBitStreamSerializable, IRemoveEvent
	{
		public int EventTime { get; set; }

		public bool ShouldBuffer()
		{
			return false;
		}

		public EventScopeKind GetKind()
		{
			return EventScopeKind.Effect;
		}

		public int TargetEventId { get; set; }

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteBool(this.WillCreateNextEvent);
			bs.WriteCompressedInt(this.TargetEventId);
			bs.WriteCompressedInt(this.TargetId);
			bs.WriteCompressedInt(this.SourceId);
			bs.WriteVector3(this.Origin);
			bs.WriteCompressedInt((int)this.DestroyReason);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.WillCreateNextEvent = bs.ReadBool();
			this.TargetEventId = bs.ReadCompressedInt();
			this.TargetId = bs.ReadCompressedInt();
			this.SourceId = bs.ReadCompressedInt();
			this.Origin = bs.ReadVector3();
			this.DestroyReason = (BaseFX.EDestroyReason)bs.ReadCompressedInt();
		}

		public override string ToString()
		{
			return string.Format("DestroyReason={0} EventTime={1} Origin={2} SourceId={3} SrvEffect={4} SrvEffectCollider={5} SrvNormal={6} SrvOtherCollider={7} SrvWasScenery={8} TargetEventId={9} TargetId={10} WillCreateNextEvent={11}", new object[]
			{
				this.DestroyReason,
				this.EventTime,
				this.Origin,
				this.SourceId,
				this.SrvEffect,
				this.SrvEffectCollider,
				this.SrvNormal,
				this.SrvOtherCollider,
				this.SrvWasScenery,
				this.TargetEventId,
				this.TargetId,
				this.WillCreateNextEvent
			});
		}

		public bool WillCreateNextEvent;

		public Vector3 Origin;

		public int TargetId = -1;

		public int SourceId = -1;

		public BaseFX.EDestroyReason DestroyReason;

		public bool SrvWasScenery;

		public bool SrvWasBarrier;

		public BaseFX SrvEffect;

		public Collider SrvEffectCollider;

		public Collider SrvOtherCollider;

		public Vector3 SrvNormal;

		public int SrvPlayerID;
	}
}
