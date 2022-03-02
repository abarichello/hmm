using System;
using System.Linq;
using HeavyMetalMachines.Localization.Business;

namespace HeavyMetalMachines.Localization
{
	public class GetSupportedLanguages : IGetSupportedLanguages
	{
		public string[] GetAllLocale()
		{
			return (from code in Language.AvailableLanguages
			select code.ToString()).ToArray<string>();
		}
	}
}
