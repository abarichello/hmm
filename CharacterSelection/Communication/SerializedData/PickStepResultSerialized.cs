using System;
using HeavyMetalMachines.CharacterSelection.Picking;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication.SerializedData
{
	public class PickStepResultSerialized : IBitStreamSerializable
	{
		public PickStepResult Data { get; set; }

		public void WriteToBitStream(BitStream bs)
		{
			bs.WriteInt(this.Data.PickConfirmations.Length);
			foreach (PickConfirmation pickConfirmation in this.Data.PickConfirmations)
			{
				bs.WriteBitSerializable<PickConfirmationSerialized>(new PickConfirmationSerialized
				{
					PickConfirmation = pickConfirmation
				});
			}
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.Data = new PickStepResult
			{
				PickConfirmations = new PickConfirmation[bs.ReadInt()]
			};
			for (int i = 0; i < this.Data.PickConfirmations.Length; i++)
			{
				PickConfirmationSerialized pickConfirmationSerialized = new PickConfirmationSerialized();
				bs.ReadBitSerializable<PickConfirmationSerialized>(ref pickConfirmationSerialized);
				this.Data.PickConfirmations[i] = pickConfirmationSerialized.PickConfirmation;
			}
		}
	}
}
