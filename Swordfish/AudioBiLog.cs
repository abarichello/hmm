using System;
using Hoplon.Serialization;

namespace HeavyMetalMachines.Swordfish
{
	[Serializable]
	public class AudioBiLog : JsonSerializeable<AudioBiLog>
	{
		public int Master { get; set; }

		public int Music { get; set; }

		public int GameplaySfx { get; set; }

		public int AmbientSfx { get; set; }

		public int VoiceOver { get; set; }

		public int VoiceChat { get; set; }

		public int Announcer { get; set; }

		public int AnnouncerId { get; set; }
	}
}
