using System;

namespace HeavyMetalMachines.Battlepass
{
	public interface IBattlepassBuyUiActions
	{
		void OnBuyLevelsButtonClick(int quantity, int targetLevel);

		void OnUnlockPremiumButtonClick(bool fromMissionWindow);
	}
}
