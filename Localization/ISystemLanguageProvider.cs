using System;

namespace HeavyMetalMachines.Localization
{
	public interface ISystemLanguageProvider
	{
		LanguageCode GetCurrent();
	}
}
