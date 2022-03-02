using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.CompetitiveMode.View.Divisions
{
	public interface ICompetitiveSubdivisionView
	{
		IDynamicImage IconImage { get; }

		ILabel NameLabel { get; }

		ILabel RangeLabel { get; }

		IImage ArrowImage { get; }
	}
}
