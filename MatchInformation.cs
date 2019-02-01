using System;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class MatchInformation : IMatchInformation
	{
		public Guid MatchId { get; set; }

		public string Version { get; set; }

		public MatchData Data { get; set; }

		public IMatchBuffer States { get; set; }

		public IMatchBuffer KeyFrames { get; set; }
	}
}
