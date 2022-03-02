using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public interface ICompetitiveUnlockAnnouncementView
	{
		IActivatable Group { get; }

		IAnimation ParentShowAnimation { get; }

		IAnimation ParentHideAnimation { get; }

		IAnimation ShowAnimation { get; }

		IAnimation HideAnimation { get; }

		IAnimation IdleAnimation { get; }

		IButton ConfirmButton { get; }

		IAnimation ShowConfirmButtonAnimation { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }
	}
}
