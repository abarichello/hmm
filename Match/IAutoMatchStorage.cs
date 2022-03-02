using System;
using HeavyMetalMachines.Matches.DataTransferObjects;

namespace HeavyMetalMachines.Match
{
	public interface IAutoMatchStorage
	{
		MatchKind LastMatchKind { get; set; }
	}
}
