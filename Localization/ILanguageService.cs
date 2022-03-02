using System;

namespace HeavyMetalMachines.Localization
{
	public interface ILanguageService
	{
		LanguageCode DefaultLanguage { get; }

		bool UseSystemLanguageAsDefault { get; }

		string CurrentLocale { get; }

		void Initialize();

		bool IsLanguageSupported(LanguageCode languageCode);

		void SwitchLanguage(LanguageCode languageCode);
	}
}
