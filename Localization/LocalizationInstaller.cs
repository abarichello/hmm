using System;
using HeavyMetalMachines.Localization.Business;
using Hoplon.Localization;
using Hoplon.Localization.TranslationTable;
using Pocketverse.Localization;
using Zenject;

namespace HeavyMetalMachines.Localization
{
	public class LocalizationInstaller : MonoInstaller<LocalizationInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<ILocalizeKey>().To<LocalizeKey>().AsTransient();
			base.Container.Bind<ILocalizeDateTime>().To<LegacyLocalizeDateTime>().AsTransient();
			base.Container.Bind<ILocalizeDecimalNumber>().To<LegacyLocalizeDecimalNumber>().AsTransient();
			base.Container.Bind<IContextInfo>().FromMethod((InjectContext _) => Language.MainTranslatedLanguage);
			base.Container.Bind<IGetLocale>().FromMethod((InjectContext _) => Language.MainTranslatedLanguage);
			base.Container.Bind<ILanguageService>().To<LanguageService>().AsTransient();
			base.Container.Bind<ISystemLanguageProvider>().To<UnitySystemLanguageProvider>().AsTransient();
			base.Container.Bind<IGetLanguageLocale>().To<GetLanguageLocale>().AsTransient();
			base.Container.Bind<ILocalizeInputKey>().To<LocalizeInputKey>().AsTransient();
			base.Container.Bind<IGetSupportedLanguages>().To<GetSupportedLanguages>().AsTransient();
		}
	}
}
