using System;

namespace HeavyMetalMachines.Battlepass
{
	public interface IBattlepassView
	{
		void Setup(BattlepassViewData viewData);

		void SetVisibility(bool isVisible, bool hasRewardsToClaim, bool imediate);

		bool IsVisible();

		void RefreshData(BattlepassViewData viewData);

		void RewardWindowClosed();

		void EnableInteraction();

		void TryToOpenPremiumShop();
	}
}
