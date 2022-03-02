using System;
using System.Globalization;
using HeavyMetalMachines.Utils;
using Hoplon.Logging;
using UnityEngine;

namespace HeavyMetalMachines.Localization
{
	public class UnitySystemLanguageProvider : ISystemLanguageProvider
	{
		public UnitySystemLanguageProvider(ILogger<UnitySystemLanguageProvider> logger)
		{
			this._logger = logger;
		}

		public LanguageCode GetCurrent()
		{
			SystemLanguage systemLanguage = Application.systemLanguage;
			if (systemLanguage != 42)
			{
				return Language.LanguageNameToCode(Application.systemLanguage);
			}
			this._logger.Warn("Unity returned 'Unknown' as system language. Trying getting the language from System Culture.");
			LanguageCode languageFromSystemCulture = UnitySystemLanguageProvider.GetLanguageFromSystemCulture();
			if (languageFromSystemCulture == LanguageCode.N)
			{
				this._logger.Warn("Could not get system language by SystemCulture.");
				return LanguageCode.N;
			}
			return languageFromSystemCulture;
		}

		private static LanguageCode GetLanguageFromSystemCulture()
		{
			CultureInfo systemCulture = CultureUtils.GetSystemCulture();
			if (systemCulture.Equals(CultureInfo.InvariantCulture))
			{
				return LanguageCode.N;
			}
			string twoLetterISOLanguageName = systemCulture.TwoLetterISOLanguageName;
			return LocalizationSettings.GetLanguageEnum(twoLetterISOLanguageName);
		}

		private readonly ILogger<UnitySystemLanguageProvider> _logger;
	}
}
