using System;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class SystemCheckSettings : GameHubScriptableObject
	{
		public int MemoryMinimumRequirement;

		public int MemoryWarningRequirement;

		public float LowSpecWindowTimeout;

		public string TitleTextDraft;

		public string QuestionTextDraft;

		public string OkButtonTextDraft;
	}
}
