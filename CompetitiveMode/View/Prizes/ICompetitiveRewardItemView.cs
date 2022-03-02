using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.CompetitiveMode.View.Prizes
{
	public interface ICompetitiveRewardItemView
	{
		IDynamicImage ThumbnailImage { get; }

		IToggle Toggle { get; }

		void Destroy();
	}
}
