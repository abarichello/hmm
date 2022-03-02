using System;
using System.Collections.Generic;
using Hoplon.Localization.TranslationTable;
using Hoplon.Logging;

namespace HeavyMetalMachines.Localization
{
	public class GetLanguageLocale : IGetLanguageLocale
	{
		public GetLanguageLocale(ILanguageService languageService, ILocalizeKey localizeKey, ILogger<GetLanguageLocale> logger)
		{
			this._languageService = languageService;
			this._localizeKey = localizeKey;
			this._logger = logger;
		}

		public string GetCurrent()
		{
			return this._languageService.CurrentLocale;
		}

		public string GetLocalizedCurrent()
		{
			string currentLocale = this._languageService.CurrentLocale;
			return this.GetLocalized(currentLocale);
		}

		public string GetLocalized(string locale)
		{
			if (!GetLanguageLocale.LocaleDrafts.ContainsKey(locale))
			{
				this._logger.WarnFormat("Could not find translation for language with locale {0}.", new object[]
				{
					locale
				});
				return string.Format("Unknown ({0})", locale);
			}
			return this._localizeKey.Get(GetLanguageLocale.LocaleDrafts[locale], TranslationContext.Options);
		}

		private readonly ILanguageService _languageService;

		private readonly ILocalizeKey _localizeKey;

		private readonly ILogger<GetLanguageLocale> _logger;

		private static readonly Dictionary<string, string> LocaleDrafts = new Dictionary<string, string>
		{
			{
				"EN",
				"GAME_LANGUAGE_VALUES_EN"
			},
			{
				"PT",
				"GAME_LANGUAGE_VALUES_BR"
			},
			{
				"RU",
				"GAME_LANGUAGE_VALUES_RU"
			},
			{
				"DE",
				"GAME_LANGUAGE_VALUES_DE"
			},
			{
				"FR",
				"GAME_LANGUAGE_VALUES_FR"
			},
			{
				"ES",
				"GAME_LANGUAGE_VALUES_ES"
			},
			{
				"TR",
				"GAME_LANGUAGE_VALUES_TR"
			},
			{
				"PL",
				"GAME_LANGUAGE_VALUES_PL"
			}
		};
	}
}
