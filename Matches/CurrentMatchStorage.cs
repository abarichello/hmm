using System;

namespace HeavyMetalMachines.Matches
{
	public class CurrentMatchStorage : ICurrentMatchStorage
	{
		public Match? CurrentMatch { get; set; }
	}
}
