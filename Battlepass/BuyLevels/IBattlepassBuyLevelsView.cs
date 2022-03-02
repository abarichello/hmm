using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;

namespace HeavyMetalMachines.Battlepass.BuyLevels
{
	public interface IBattlepassBuyLevelsView
	{
		IActivatable WindowActivatable { get; }

		IAnimation ShowWindowAnimation { get; }

		IAnimation HideWindowAnimation { get; }

		ILabel TargetLevelLabel { get; }

		ILabel SelectedLevelsPriceLabel { get; }

		IButton PurchaseSelectedButton { get; }

		ILabel AllLevelsPriceLabel { get; }

		IButton PurchaseAllButton { get; }

		IButton BackButton { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }
	}
}
