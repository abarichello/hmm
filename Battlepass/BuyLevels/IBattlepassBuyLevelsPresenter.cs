using System;
using HeavyMetalMachines.Presenting;
using UniRx;

namespace HeavyMetalMachines.Battlepass.BuyLevels
{
	public interface IBattlepassBuyLevelsPresenter : IPresenter
	{
		void SetupLevelValues(int selectedLevelsPrice, int targetLevel, int allLevelsPrice);

		IObservable<Unit> ObserveBuyCurrentLevel();

		IObservable<Unit> ObserveBuyAllLevels();
	}
}
