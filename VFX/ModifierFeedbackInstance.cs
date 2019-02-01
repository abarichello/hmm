using System;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;

namespace HeavyMetalMachines.VFX
{
	[Serializable]
	public class ModifierFeedbackInstance : IBitStreamSerializable
	{
		public ModifierFeedbackInfo FeedbackInfo
		{
			get
			{
				if (this._feedback)
				{
					return this._feedback;
				}
				ModifierFeedbackInfo.Feedbacks.TryGetValue(this.ModifierFeedbackId, out this._feedback);
				return this._feedback;
			}
		}

		public void Copy(ModifierFeedbackInstance other)
		{
			if (other == null)
			{
				return;
			}
			this.StartTime = other.StartTime;
			this.EndTime = other.EndTime;
			this.Charges = other.Charges;
			this.GadgetSlot = other.GadgetSlot;
		}

		public override bool Equals(object obj)
		{
			ModifierFeedbackInstance modifierFeedbackInstance = obj as ModifierFeedbackInstance;
			return modifierFeedbackInstance != null && (this.ModifierFeedbackId == modifierFeedbackInstance.ModifierFeedbackId && this.EventId == modifierFeedbackInstance.EventId) && this.Causer == modifierFeedbackInstance.Causer;
		}

		public override int GetHashCode()
		{
			return this.ModifierFeedbackId * 397 ^ this.EventId;
		}

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteCompressedInt(this.InstanceId);
			bs.WriteCompressedInt(this.ModifierFeedbackId);
			bs.WriteCompressedInt(this.EventId);
			bs.WriteCompressedInt(this.StartTime);
			bs.WriteCompressedInt(this.EndTime);
			bs.WriteCompressedInt(this.Causer);
			bs.WriteCompressedInt(this.Target);
			bs.WriteCompressedInt((int)this.GadgetSlot);
			bs.WriteBits(7, this.Charges);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.InstanceId = bs.ReadCompressedInt();
			this.ModifierFeedbackId = bs.ReadCompressedInt();
			this.EventId = bs.ReadCompressedInt();
			this.StartTime = bs.ReadCompressedInt();
			this.EndTime = bs.ReadCompressedInt();
			this.Causer = bs.ReadCompressedInt();
			this.Target = bs.ReadCompressedInt();
			this.GadgetSlot = (GadgetSlot)bs.ReadCompressedInt();
			this.Charges = bs.ReadBits(7);
		}

		public int InstanceId;

		public int ModifierFeedbackId;

		public int EventId;

		public int StartTime;

		public int EndTime;

		public int Charges;

		public int Causer;

		public int Target;

		public GadgetSlot GadgetSlot;

		private ModifierFeedbackInfo _feedback;

		public ModifierFeedback FeedbackEffect;
	}
}
