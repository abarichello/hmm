using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;

namespace HeavyMetalMachines.Battlepass
{
	public interface IBattlepassInfoView
	{
		IButton PrevPageButton { get; }

		IButton NextPageButton { get; }

		IButton BackButton { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }

		void SetVisibility(bool isVisible);

		bool IsVisible();
	}
}
