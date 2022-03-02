using System;

namespace HeavyMetalMachines.Storyteller.Presenting
{
	public class StorytellerMatchMember
	{
		public string UniversalId { get; set; }

		public string PlayerName { get; set; }

		public string PublisherUsername { get; set; }

		public bool ShowPublisherUsername { get; set; }

		public long PlayerId { get; set; }

		public long? PlayerTag { get; set; }

		public int PublisherId { get; set; }

		public bool IsBot
		{
			get
			{
				return this.UniversalId == "-1";
			}
		}
	}
}
