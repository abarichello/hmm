using System;

namespace HeavyMetalMachines.Matches
{
	public interface ICurrentMatchStorage
	{
		Match? CurrentMatch { get; set; }
	}
}
