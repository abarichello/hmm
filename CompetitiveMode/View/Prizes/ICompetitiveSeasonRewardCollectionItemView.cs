using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.CompetitiveMode.View.Prizes
{
	public interface ICompetitiveSeasonRewardCollectionItemView
	{
		IDynamicImage ThumbnailImage { get; }

		ILabel NameLabel { get; }
	}
}
