using System;

namespace HeavyMetalMachines.Audio
{
	[Serializable]
	public struct VoiceOverLine
	{
		public void Preload()
		{
			if (this.VoiceLine != null)
			{
				this.VoiceLine.Preload();
			}
		}

		public FMODVoiceOverAsset VoiceLine;

		public bool PlayOnTutorial;
	}
}
