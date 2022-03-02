using System;

namespace HeavyMetalMachines.Localization
{
	public class ManualInitializeLocalization : IInitializeLocalization
	{
		public ManualInitializeLocalization(ILanguageService languageService)
		{
			this._languageService = languageService;
		}

		public LanguageCode Language { get; set; }

		public LocalizationInitializationResult Initialize()
		{
			this._languageService.Initialize();
			this._languageService.SwitchLanguage(this.Language);
			return LocalizationInitializationResult.LanguageUnchanged;
		}

		private readonly ILanguageService _languageService;
	}
}
