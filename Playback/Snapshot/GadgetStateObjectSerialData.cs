using System;
using HeavyMetalMachines.Combat;
using Pocketverse;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public class GadgetStateObjectSerialData : IGadgetStateObjectSerialData, IBaseStreamSerialData<IGadgetStateObjectSerialData>
	{
		public GadgetState GadgetState { get; set; }

		public EffectState EffectState { get; set; }

		public long Cooldown { get; set; }

		public int Value { get; set; }

		public float Heat { get; set; }

		public int Counter { get; set; }

		public int[] AffectedIds { get; set; }

		public void Apply(IGadgetStateObjectSerialData other)
		{
			this.GadgetState = other.GadgetState;
			this.EffectState = other.EffectState;
			this.Cooldown = other.Cooldown;
			this.Value = other.Value;
			this.Heat = other.Heat;
			this.Counter = other.Counter;
			if (other.AffectedIds != null)
			{
				this.AffectedIds = new int[other.AffectedIds.Length];
				Array.Copy(other.AffectedIds, this.AffectedIds, this.AffectedIds.Length);
			}
			else
			{
				this.AffectedIds = new int[0];
			}
		}

		public void Apply(BitStream stream)
		{
			if (!stream.ReadBool())
			{
				return;
			}
			this.Cooldown = stream.ReadCompressedLong();
			this.Value = stream.ReadCompressedInt();
			this.Heat = stream.ReadTinyUFloat();
			this.Counter = stream.ReadCompressedInt();
			this.AffectedIds = stream.ReadIntArray();
			this.GadgetState = (GadgetState)stream.ReadBits(4);
			this.EffectState = (EffectState)stream.ReadBits(4);
		}
	}
}
