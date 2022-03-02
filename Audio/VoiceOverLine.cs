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

		public AudioEventAsset VoiceLine;

		public bool PlayOnTutorial;
	}
}
