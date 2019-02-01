using System;

namespace HeavyMetalMachines.Battlepass
{
	public interface IBattlepassPremiumShopComponent
	{
		void ShowPremiumShopWindow(Action<int> onUnlockPremium, Action onBuyWindowClosed);

		bool IsPremiumShopSceneLoad();

		void RegisterPremiumShopWindow(IBattlepassPremiumShopView premiumShopView);

		void OnBuyPremiumRequested(int packageIndex);

		void HidePremiumShopWindow();

		void OnHidePremiumShopWindowAnimationEnded();
	}
}
