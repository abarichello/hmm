using System;
using HeavyMetalMachines.Matches.DataTransferObjects;

namespace HeavyMetalMachines.Match
{
	public class AutoMatchStorage : IAutoMatchStorage
	{
		public MatchKind LastMatchKind { get; set; }
	}
}
