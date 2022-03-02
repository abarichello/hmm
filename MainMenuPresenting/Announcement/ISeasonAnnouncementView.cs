using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;

namespace HeavyMetalMachines.MainMenuPresenting.Announcement
{
	public interface ISeasonAnnouncementView
	{
		ICanvas MainCanvas { get; }

		IButton CloseButton { get; }

		IAnimation ShowAnimation { get; }

		IAnimation HideAnimation { get; }

		IDynamicImage BackgroundImage { get; }

		string BattlepassBackgroundImageName { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }

		string CompetitiveModeBackgroundImageName { get; }

		string MergedBackgroundImageName { get; }
	}
}
