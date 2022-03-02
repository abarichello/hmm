using System;
using HeavyMetalMachines.Achievements.DataTransferObject;
using HeavyMetalMachines.Achievements.Infra;
using HeavyMetalMachines.Achievements.Swordfish;
using HeavyMetalMachines.DependencyInjection;
using Pocketverse;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Achievements
{
	public class AchievementsInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			ZenjectInjectionBinder zenjectInjectionBinder = new ZenjectInjectionBinder(base.Container);
			new AchievementsModule(zenjectInjectionBinder).Bind();
			new SwordfishAchievementsModule(zenjectInjectionBinder).Bind();
			base.Container.Bind<IAchievementsProvider>().To<MainMenuDataAchievementsProvider>().AsTransient().When((InjectContext _) => !this._configLoader.GetBoolValue(ConfigAccess.SkipSwordfish));
			base.Container.Bind<IAchievementsProvider>().To<AchievementsInstaller.SkipSwordfishAchievementsProvider>().AsTransient().When((InjectContext _) => this._configLoader.GetBoolValue(ConfigAccess.SkipSwordfish));
		}

		[Inject]
		private IConfigLoader _configLoader;

		public class SkipSwordfishAchievementsProvider : IAchievementsProvider
		{
			public IObservable<PlayerAchievement[]> GetFromLocalPlayer()
			{
				return Observable.Return<PlayerAchievement[]>(new PlayerAchievement[0]);
			}
		}
	}
}
