using System;
using HeavyMetalMachines.Boosters.Business;
using HeavyMetalMachines.DependencyInjection;
using Zenject;

namespace HeavyMetalMachines.Boosters
{
	public class BoostersInstaller : MonoInstaller<BoostersInstaller>
	{
		public override void InstallBindings()
		{
			ZenjectInjectionBinder zenjectInjectionBinder = new ZenjectInjectionBinder(base.Container);
			zenjectInjectionBinder.BindTransient<IGetLocalPlayerXpBooster, GetLocalPlayerXpBooster>();
			zenjectInjectionBinder.BindTransient<IGetLocalPlayerFameBooster, GetLocalPlayerFameBooster>();
			zenjectInjectionBinder.BindTransient<IGetLocalPlayerFounterBooster, GetLocalPlayerFounderBooster>();
		}
	}
}
