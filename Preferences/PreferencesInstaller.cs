using System;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Preferences
{
	public class PreferencesInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			bool boolValue = this._config.GetBoolValue(ConfigAccess.SkipSwordfish);
			if (boolValue)
			{
				base.Container.Bind<ILoadPlayerPreferences>().To<SkipSwordfishLoadPlayerPreferences>().AsTransient();
			}
			else
			{
				base.Container.Bind<ILoadPlayerPreferences>().To<LoadPlayerPreferences>().AsTransient();
			}
		}

		[Inject]
		private IConfigLoader _config;
	}
}
