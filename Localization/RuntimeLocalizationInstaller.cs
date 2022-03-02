using System;
using Zenject;

namespace HeavyMetalMachines.Localization
{
	public class RuntimeLocalizationInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			if (Platform.Current.IsConsole())
			{
				base.Container.Bind<IInitializeLocalization>().To<ConsoleInitializeLocalization>().AsTransient();
			}
			else
			{
				base.Container.Bind<IInitializeLocalization>().To<LegacyInitializeLocalization>().AsTransient();
			}
		}
	}
}
