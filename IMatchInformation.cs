using System;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines
{
	public interface IMatchInformation
	{
		Guid MatchId { get; }

		string Version { get; }

		MatchData Data { get; }

		IMatchBuffer States { get; }

		IMatchBuffer KeyFrames { get; }
	}
}
