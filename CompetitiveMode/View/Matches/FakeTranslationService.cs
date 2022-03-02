using System;
using Hoplon.Localization.TranslationTable;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class FakeTranslationService : ILocalizeKey
	{
		public string Get(string draft, ContextTag context)
		{
			return draft;
		}

		public string GetFormatted(string draft, ContextTag context, params object[] args)
		{
			return draft;
		}
	}
}
