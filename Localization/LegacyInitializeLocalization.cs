using System;
using HeavyMetalMachines.Net.Infra;
using HeavyMetalMachines.Options;
using Hoplon.Assertions;
using Hoplon.Logging;
using Pocketverse;

namespace HeavyMetalMachines.Localization
{
	public class LegacyInitializeLocalization : IInitializeLocalization
	{
		public LegacyInitializeLocalization(ILanguageService languageService, ISystemLanguageProvider systemLanguageProvider, INetwork network, IConfigLoader configLoader, ILogger<LegacyInitializeLocalization> logger)
		{
			Assert.ConstructorParametersAreNotNull(new object[]
			{
				languageService,
				systemLanguageProvider,
				network,
				configLoader,
				logger
			});
			this._languageService = languageService;
			this._systemLanguageProvider = systemLanguageProvider;
			this._network = network;
			this._configLoader = configLoader;
			this._logger = logger;
		}

		public LocalizationInitializationResult Initialize()
		{
			this._languageService.Initialize();
			LanguageCode languageCode = this.DecideLanguage();
			this._languageService.SwitchLanguage(languageCode);
			return LocalizationInitializationResult.LanguageUnchanged;
		}

		private LanguageCode DecideLanguage()
		{
			if (this._network.IsServer())
			{
				return this.GetServerLanguage();
			}
			LanguageCode result;
			if (this.TryGettingConfiguredLanguage(out result))
			{
				return result;
			}
			if (this.TryGettingSystemLanguage(out result))
			{
				return result;
			}
			return this.GetDefaultLanguage();
		}

		private bool TryGettingConfiguredLanguage(out LanguageCode configuredLanguage)
		{
			configuredLanguage = LanguageCode.N;
			int intSetting = this._configLoader.GetIntSetting(GameOptions.GameOptionPrefs.OPTIONS_GAME_LANGUAGE.ToString(), -1);
			if (intSetting == -1)
			{
				return false;
			}
			configuredLanguage = LanguageLocalizationOptions.LanguageIndexToCode(intSetting);
			if (!this._languageService.IsLanguageSupported(configuredLanguage))
			{
				return false;
			}
			this._logger.InfoFormat("Switching to previously configured language '{0}'.", new object[]
			{
				configuredLanguage
			});
			return true;
		}

		private bool TryGettingSystemLanguage(out LanguageCode systemLanguage)
		{
			systemLanguage = LanguageCode.N;
			if (!this._languageService.UseSystemLanguageAsDefault)
			{
				return false;
			}
			systemLanguage = this._systemLanguageProvider.GetCurrent();
			if (!this._languageService.IsLanguageSupported(systemLanguage))
			{
				return false;
			}
			this._logger.InfoFormat("Could not set previously configured language. Switching to system language '{0}'.", new object[]
			{
				systemLanguage
			});
			return true;
		}

		private LanguageCode GetServerLanguage()
		{
			this._logger.InfoFormat("Running on server. Switching to default language '{0}'.", new object[]
			{
				this._languageService.DefaultLanguage
			});
			return this._languageService.DefaultLanguage;
		}

		private LanguageCode GetDefaultLanguage()
		{
			this._logger.InfoFormat("Could not set previously configured language. Switching to default language '{0}'.", new object[]
			{
				this._languageService.DefaultLanguage
			});
			return this._languageService.DefaultLanguage;
		}

		private readonly ILanguageService _languageService;

		private readonly ISystemLanguageProvider _systemLanguageProvider;

		private readonly INetwork _network;

		private readonly IConfigLoader _configLoader;

		private readonly ILogger<LegacyInitializeLocalization> _logger;
	}
}
