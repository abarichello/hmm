using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public interface ICompetitiveUnlockView
	{
		IActivatable UnlockMatchesGroup { get; }

		ILabel MatchesPlayedLabel { get; }

		ILabel TotalMatchesNeededLabel { get; }
	}
}
