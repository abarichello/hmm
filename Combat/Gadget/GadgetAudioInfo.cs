using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class GadgetAudioInfo : IBitStreamSerializable
	{
		public void WriteToBitStream(BitStream bs)
		{
			GadgetAudioInfo.WriteAudioEvent(this.FireAudio, bs);
			GadgetAudioInfo.WriteAudioEvent(this.HitAudio, bs);
			GadgetAudioInfo.WriteAudioEvent(this.FadeAudio, bs);
			GadgetAudioInfo.WriteAudioEvent(this.LoopAudio, bs);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.FireAudio = GadgetAudioInfo.ReadAudioEvent(bs);
			this.HitAudio = GadgetAudioInfo.ReadAudioEvent(bs);
			this.FadeAudio = GadgetAudioInfo.ReadAudioEvent(bs);
			this.LoopAudio = GadgetAudioInfo.ReadAudioEvent(bs);
		}

		private static void WriteAudioEvent(string e, BitStream bs)
		{
			Guid value = GuidUtils.TryParse(e);
			bs.WriteGuid(value);
		}

		private static string ReadAudioEvent(BitStream bs)
		{
			return bs.ReadGuid().ToString();
		}

		[AudioDrawer]
		public string FireAudio;

		public FMODAsset tmpFireAudio;

		[AudioDrawer]
		public string HitAudio;

		public FMODAsset tmpHitAudio;

		[AudioDrawer]
		public string FadeAudio;

		public FMODAsset tmpFadeAudio;

		[AudioDrawer]
		public string LoopAudio;

		public FMODAsset tmpLoopAudio;
	}
}
