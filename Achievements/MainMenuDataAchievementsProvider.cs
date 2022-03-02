using System;
using HeavyMetalMachines.Achievements.DataTransferObject;
using HeavyMetalMachines.Achievements.Infra;
using HeavyMetalMachines.LegacyStorage;
using UniRx;

namespace HeavyMetalMachines.Achievements
{
	public class MainMenuDataAchievementsProvider : IAchievementsProvider
	{
		public MainMenuDataAchievementsProvider(MainMenuDataStorage mainMenuDataStorage)
		{
			this._mainMenuDataStorage = mainMenuDataStorage;
		}

		public IObservable<PlayerAchievement[]> GetFromLocalPlayer()
		{
			return Observable.Return<PlayerAchievement[]>(this._mainMenuDataStorage.LatestMainMenuData.PlayerAchievements);
		}

		private readonly MainMenuDataStorage _mainMenuDataStorage;
	}
}
