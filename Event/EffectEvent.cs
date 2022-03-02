using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Event
{
	[Serializable]
	public class EffectEvent : IEventContent, IBitStreamSerializable
	{
		public override string ToString()
		{
			return string.Format("Effect[Source={0} Res={2} Tevid={1}]", this.SourceId, this.TargetEventId, (this.EffectInfo != null) ? this.EffectInfo.Effect : "null");
		}

		public int EventTime { get; set; }

		public EventScopeKind GetKind()
		{
			return EventScopeKind.Effect;
		}

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteVector3(this.Origin);
			bs.WriteVector3(this.Target);
			bs.WriteByteVector3(this.Direction);
			bs.WriteShortVector2NonNormalized(this.MousePosition);
			bs.WriteCompressedInt(this.PreviousEventId);
			bs.WriteCompressedInt(this.SourceId);
			bs.WriteCompressedInt(this.UpdateTime);
			bs.WriteVector3(this.CurrentPosition);
			bs.WriteCompressedInt(this.TargetEventId);
			bs.WriteCompressedInt(this.TargetId);
			bs.WriteBool(this.FirstPackageSent);
			bs.WriteCompressedInt((int)this.SourceSlot);
			bs.WriteCompressedFixedPoint(this.MoveSpeed, 2);
			bs.WriteCompressedFixedPoint(this.Range, 2);
			bs.WriteCompressedFixedPoint(this.LifeTime, 2);
			if (this.CustomVar != 0)
			{
				bs.WriteBool(true);
				bs.WriteByte(this.CustomVar);
			}
			else
			{
				bs.WriteBool(false);
			}
			bs.WriteByte(this.TagMask);
			if (this.EffectInfo == null && this.released)
			{
				EffectEvent.Log.FatalFormat("Empty effect info on {0}.", new object[]
				{
					this.debugName
				});
			}
			bs.WriteCompressedInt(this.EffectInfo.EffectId);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.Origin = bs.ReadVector3();
			this.Target = bs.ReadVector3();
			this.Direction = bs.ReadByteVector3();
			this.MousePosition = bs.ReadShortVector2NonNormalized();
			this.PreviousEventId = bs.ReadCompressedInt();
			this.SourceId = bs.ReadCompressedInt();
			this.UpdateTime = bs.ReadCompressedInt();
			this.CurrentPosition = bs.ReadVector3();
			this.TargetEventId = bs.ReadCompressedInt();
			this.TargetId = bs.ReadCompressedInt();
			this.FirstPackageSent = bs.ReadBool();
			this.SourceSlot = (GadgetSlot)bs.ReadCompressedInt();
			this.MoveSpeed = bs.ReadCompressedFixedPoint(2);
			this.Range = bs.ReadCompressedFixedPoint(2);
			this.LifeTime = bs.ReadCompressedFixedPoint(2);
			bool flag = bs.ReadBool();
			this.CustomVar = ((!flag) ? 0 : bs.ReadByte());
			this.TagMask = bs.ReadByte();
			int num = bs.ReadCompressedInt();
			if (!GadgetInfo.EffectMap.TryGetValue(num, out this.EffectInfo))
			{
				EffectEvent.Log.ErrorFormat("Failed to load effect={0}", new object[]
				{
					num
				});
			}
		}

		public bool ShouldBuffer()
		{
			return this.EffectInfo != null && !this.EffectInfo.Instantaneous;
		}

		public void CopyInfo(FXInfo info)
		{
			this.EffectInfo = info;
			if (info == null)
			{
				EffectEvent.Log.Fatal("An null FXInfo has been passed to CopyInfo!");
			}
			else
			{
				this.debugName = info.Effect;
			}
		}

		public Quaternion StartingRotation
		{
			get
			{
				if (!(this.Direction == Vector3.zero))
				{
					return Quaternion.LookRotation(this.Direction, Vector3.up);
				}
				if ((this.Target - this.Origin).sqrMagnitude < 0.05f)
				{
					return Quaternion.identity;
				}
				return Quaternion.LookRotation(this.Target - this.Origin, Vector3.up);
			}
		}

		public long DeathTime
		{
			get
			{
				return (long)this.EventTime + (long)(this.LifeTime * 1000f);
			}
		}

		public void Release()
		{
			this.released = true;
			this.Modifiers = null;
			this.SourceGadget = null;
			this.SourceCombat = null;
			this.EffectInfo = null;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(EffectEvent));

		private bool released;

		private string debugName;

		public FXInfo EffectInfo;

		public int PreviousEventId = -1;

		public Vector3 Origin;

		public Vector3 Target;

		public Vector3 Direction;

		public Vector2 MousePosition;

		public int SourceId = -1;

		public GadgetSlot SourceSlot;

		public int TargetId = -1;

		public int UpdateTime;

		public Vector3 CurrentPosition;

		public byte CustomVar;

		public byte TagMask;

		public float MoveSpeed;

		public float Range;

		public float LifeTime;

		public int TargetEventId;

		public bool FirstPackageSent;

		public ModifierData[] Modifiers;

		public ModifierData[] ExtraModifiers;

		public CombatObject SourceCombat;

		public GadgetBehaviour SourceGadget;
	}
}
