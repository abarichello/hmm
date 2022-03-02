using System;
using HeavyMetalMachines.Options;
using Hoplon.Logging;
using Pocketverse;

namespace HeavyMetalMachines.Localization
{
	public class ConsoleInitializeLocalization : IInitializeLocalization
	{
		public ConsoleInitializeLocalization(ILanguageService languageService, ISystemLanguageProvider systemLanguageProvider, IConfigLoader configLoader, ILogger<ConsoleInitializeLocalization> logger)
		{
			this._languageService = languageService;
			this._systemLanguageProvider = systemLanguageProvider;
			this._configLoader = configLoader;
			this._logger = logger;
		}

		public LocalizationInitializationResult Initialize()
		{
			this._languageService.Initialize();
			LanguageCode current = this._systemLanguageProvider.GetCurrent();
			LanguageCode configuredLanguage = this.GetConfiguredLanguage();
			this._logger.InfoFormat("System Language '{0}'.", new object[]
			{
				current
			});
			this._logger.InfoFormat("Configured Language '{0}'.", new object[]
			{
				configuredLanguage
			});
			if (!this._languageService.IsLanguageSupported(current))
			{
				this._logger.Info("SwitchToUnsupportedSystemLanguage");
				return this.SwitchWithUnsupportedSystemLanguage(configuredLanguage);
			}
			if (configuredLanguage == LanguageCode.N)
			{
				this._logger.Info("SwitchWithSupportedSystemLanguageFirstTime");
				return this.SwitchWithSupportedSystemLanguageFirstTime(current);
			}
			this._logger.Info("SwitchToSystemLanguage");
			return this.SwitchWithSupportedSystemLanguage(configuredLanguage, current);
		}

		private LocalizationInitializationResult SwitchWithSupportedSystemLanguageFirstTime(LanguageCode systemLanguage)
		{
			this.SwitchToLanguage(systemLanguage);
			return LocalizationInitializationResult.LanguageUnchanged;
		}

		private LocalizationInitializationResult SwitchWithSupportedSystemLanguage(LanguageCode configuredLanguage, LanguageCode systemLanguage)
		{
			if (configuredLanguage == systemLanguage)
			{
				this.SwitchToLanguage(configuredLanguage);
				return LocalizationInitializationResult.LanguageUnchanged;
			}
			this.SwitchToLanguage(systemLanguage);
			return LocalizationInitializationResult.ChangedToSystemLanguage;
		}

		private LocalizationInitializationResult SwitchWithUnsupportedSystemLanguage(LanguageCode configuredLanguage)
		{
			this.SwitchToLanguage(this._languageService.DefaultLanguage);
			if (configuredLanguage == this._languageService.DefaultLanguage)
			{
				return LocalizationInitializationResult.LanguageUnchanged;
			}
			return LocalizationInitializationResult.FallbackedToDefaultLanguage;
		}

		private void SwitchToLanguage(LanguageCode languageCode)
		{
			this._languageService.SwitchLanguage(languageCode);
			int num = LanguageLocalizationOptions.LanguageCodeToIndex(languageCode);
			this._configLoader.SetSetting(GameOptions.GameOptionPrefs.OPTIONS_GAME_LANGUAGE.ToString(), num.ToString());
		}

		private LanguageCode GetConfiguredLanguage()
		{
			int intSetting = this._configLoader.GetIntSetting(GameOptions.GameOptionPrefs.OPTIONS_GAME_LANGUAGE.ToString(), -1);
			return LanguageLocalizationOptions.LanguageIndexToCode(intSetting);
		}

		private readonly ILanguageService _languageService;

		private readonly ISystemLanguageProvider _systemLanguageProvider;

		private readonly IConfigLoader _configLoader;

		private readonly ILogger<ConsoleInitializeLocalization> _logger;
	}
}
