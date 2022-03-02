using System;

namespace HeavyMetalMachines.Localization
{
	public enum LocalizationInitializationResult
	{
		LanguageUnchanged,
		ChangedToSystemLanguage,
		FallbackedToDefaultLanguage
	}
}
