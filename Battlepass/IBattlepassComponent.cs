using System;
using HeavyMetalMachines.Battlepass.Business;
using HeavyMetalMachines.Boosters.Business;
using HeavyMetalMachines.Store.Business;

namespace HeavyMetalMachines.Battlepass
{
	public interface IBattlepassComponent
	{
		BattlepassViewData GetBattlepassViewData();

		void SetStoreBusinessFactory(IStoreBusinessFactory storeBusinessFactory);

		void SetIGetLocalPlayerXpBooster(IGetLocalPlayerXpBooster playerXpBooster);

		void SetIGetBattlepassSeason(IGetBattlepassSeason getBattlepassSeason);

		void RegisterView(ILegacyBattlepassView view);

		void UnregisterView();

		void ShowMetalpassPremiumShopWindow();

		void MetalpassBuyLevelRequest(int level, Action onRequestOk, Action onBuyWindowClosed);

		void MarkMissionsAsSeen();

		void SetLevelFake(int level);

		void GivePremiumFake();
	}
}
