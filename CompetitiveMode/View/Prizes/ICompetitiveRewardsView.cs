using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;

namespace HeavyMetalMachines.CompetitiveMode.View.Prizes
{
	public interface ICompetitiveRewardsView
	{
		ICanvas MainCanvas { get; }

		ITitle Title { get; }

		IAnimation ShowAnimation { get; }

		IAnimation HideAnimation { get; }

		IButton BackButton { get; }

		ILabel DivisionNameLabel { get; }

		ILabel DivisionScoreIntervalLabel { get; }

		IButton PreviousDivisionButton { get; }

		IButton NextDivisionButton { get; }

		IAnimation DivisionShowAnimation { get; }

		IAnimation DivisionHideAnimation { get; }

		IItemPreviewer ItemPreviewer { get; }

		IActivatable ItemPreviewerActivatable { get; }

		IActivatable[] DivisionsPreviews { get; }

		IActivatable TopPlacementRewardsObservation { get; }

		IDynamicImage TemporaryPreviewImage { get; }

		string[] TemporaryDivisionsPreviewsImageNames { get; }

		ILabel TemporaryPrizeListLabel { get; }

		IAnimation TemporaryPreviewShowAnimation { get; }

		IAnimation TemporaryPreviewHideAnimation { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }

		ICompetitiveRewardItemView CreateAndAddItem();
	}
}
