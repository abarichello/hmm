using System;

namespace HeavyMetalMachines.Localization
{
	public interface IGetLanguageLocale
	{
		string GetCurrent();

		string GetLocalizedCurrent();

		string GetLocalized(string locale);
	}
}
