using System;

namespace HeavyMetalMachines.Utils
{
	public interface ISearchable
	{
		bool MatchesQuery(string query);
	}
}
