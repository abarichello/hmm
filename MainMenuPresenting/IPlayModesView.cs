using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public interface IPlayModesView
	{
		IActivatable Group { get; }

		IActivatable CompetitiveGroup { get; }

		IButton OpenCompetitiveModeInfoButton { get; }

		IButton OpenTrainingModeButton { get; }

		IActivatable OpenCompetitiveModeInfoActivatable { get; }

		IButton BackButton { get; }

		IAlpha[] ModesArtsAlphas { get; }

		IAlpha RootPanel { get; }

		IActivatable RootObject { get; }

		IAnimation ModesAnimationIn { get; }

		IAnimation BackGroundAnimatinIn { get; }

		IAnimation ModesAnimationOut { get; }

		IAnimation BackGroundAnimatinOut { get; }

		IButton OpenNormalModeButton { get; }

		IButton OpenCustomModeButton { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }

		IUiNavigationAxisSelector UiNavigationAxisSelector { get; }

		IButton RegionButton { get; }

		ILabel CrossplayActivatedLabel { get; }
	}
}
