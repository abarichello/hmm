using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;

namespace HeavyMetalMachines.CompetitiveMode.View.Prizes
{
	public interface ICompetitiveSeasonRewardsCollectionView
	{
		ICanvas MainCanvas { get; }

		ILabel SeasonNameLabel { get; }

		IAnimation ShowAnimation { get; }

		IAnimation HideAnimation { get; }

		IActivatable[] DivisionGroups { get; }

		ILabel DivisionNameLabel { get; }

		ILabel ScoreLabel { get; }

		IButton ConfirmButton { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }

		IDynamicImage[] DivisionImages { get; }

		ICompetitiveSeasonRewardCollectionItemView CreateAndAddItem();
	}
}
