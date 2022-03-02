using System;
using System.Collections.Generic;
using System.Linq;
using Hoplon.Localization.TranslationTable;
using Hoplon.Logging;
using Hoplon.ToggleableFeatures;

namespace HeavyMetalMachines.Localization
{
	public class LanguageService : ILanguageService
	{
		public LanguageService(ILogger<LocalizationXmlLoader> localizationXmlLoaderLogger, ILogger<TranslatedLanguage> translatedLanguageLogger, IIsFeatureToggled isFeatureToggled)
		{
			this._localizationXmlLoaderLogger = localizationXmlLoaderLogger;
			this._translatedLanguageLogger = translatedLanguageLogger;
			this._isFeatureToggled = isFeatureToggled;
		}

		public LanguageCode DefaultLanguage
		{
			get
			{
				return LocalizationSettings.GetLanguageEnum(Language.Settings.defaultLangCode);
			}
		}

		public bool UseSystemLanguageAsDefault
		{
			get
			{
				return Language.Settings.useSystemLanguagePerDefault;
			}
		}

		public string CurrentLocale
		{
			get
			{
				return Language.MainTranslatedLanguage.GetLocale();
			}
		}

		public IEnumerable<LanguageCode> AvailableLanguages
		{
			get
			{
				return Language.AvailableLanguages;
			}
		}

		public void Initialize()
		{
			Language.Initialize(this._localizationXmlLoaderLogger, this._translatedLanguageLogger, this._isFeatureToggled);
		}

		public bool IsLanguageSupported(LanguageCode languageCode)
		{
			return Language.AvailableLanguages.Contains(languageCode);
		}

		public void SwitchLanguage(LanguageCode languageCode)
		{
			Language.SwitchLanguage(languageCode);
		}

		private readonly ILogger<LocalizationXmlLoader> _localizationXmlLoaderLogger;

		private readonly ILogger<TranslatedLanguage> _translatedLanguageLogger;

		private readonly IIsFeatureToggled _isFeatureToggled;
	}
}
