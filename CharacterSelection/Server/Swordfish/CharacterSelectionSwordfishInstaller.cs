using System;
using HeavyMetalMachines.CharacterSelection.Server.Infra;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Server.Swordfish
{
	public class CharacterSelectionSwordfishInstaller : MonoInstaller<CharacterSelectionSwordfishInstaller>
	{
		public override void InstallBindings()
		{
			bool boolValue = base.Container.Resolve<IConfigLoader>().GetBoolValue(ConfigAccess.SkipSwordfish);
			if (boolValue)
			{
				base.Container.BindInstance<ICharacterSelectionConfigurationsProvider>(new DefaultCharacterSelectionConfigurationProvider());
				base.Container.Bind<IValidatePlayerChoicesAndPersist>().To<SkipValidatePlayerChoicesAndPersist>().AsTransient();
			}
			else
			{
				base.Container.Bind<ICharacterSelectionConfigurationsProvider>().To<SwordfishCharacterSelectionConfigurationProvider>().AsTransient();
				base.Container.Bind<IValidatePlayerChoicesAndPersist>().To<SwordfishValidatePlayerChoicesAndPersist>().AsTransient();
			}
		}
	}
}
