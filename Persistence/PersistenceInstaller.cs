using System;
using HeavyMetalMachines.Serialization;
using Zenject;

namespace HeavyMetalMachines.Persistence
{
	public class PersistenceInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			base.Container.Bind<ILocalPlayerPreferences>().To<PlayerPrefsLocalPlayerPreferences>().AsTransient();
		}
	}
}
