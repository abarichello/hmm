using System;
using HeavyMetalMachines.Tournaments;

namespace HeavyMetalMachines.Storyteller
{
	public class StorytellerSearchableQueue
	{
		public string QueueName { get; set; }

		public string LocalizedName { get; set; }

		public bool IsTournamentTier { get; set; }

		public TournamentTier TournamentTier { get; set; }
	}
}
