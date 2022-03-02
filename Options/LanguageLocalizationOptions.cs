using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Options
{
	public static class LanguageLocalizationOptions
	{
		public static int LanguageCodeToIndex(LanguageCode code)
		{
			for (int i = 0; i < LanguageLocalizationOptions.LanguageLocalizationValues.Length; i++)
			{
				if (LanguageLocalizationOptions.LanguageLocalizationValues[i] == code)
				{
					return i;
				}
			}
			LanguageLocalizationOptions.Log.ErrorFormat("unsupported LanguageCode {0}", new object[]
			{
				code
			});
			return -1;
		}

		public static LanguageCode LanguageIndexToCode(int index)
		{
			if (index >= 0 && index < LanguageLocalizationOptions.LanguageLocalizationValues.Length)
			{
				return LanguageLocalizationOptions.LanguageLocalizationValues[index];
			}
			return LanguageCode.N;
		}

		public static int SystemLanguageToIndex(SystemLanguage systemLanguage)
		{
			LanguageCode languageCode = Language.LanguageNameToCode(systemLanguage);
			for (int i = 0; i < LanguageLocalizationOptions.LanguageLocalizationValues.Length; i++)
			{
				if (LanguageLocalizationOptions.LanguageLocalizationValues[i] == languageCode)
				{
					return i;
				}
			}
			return 0;
		}

		public static bool IsLanguageLocalizationIndexValid(int index)
		{
			return index >= 0 && index < LanguageLocalizationOptions.LanguageLocalizationValues.Length;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(LanguageLocalizationOptions));

		private static readonly LanguageCode[] LanguageLocalizationValues = new LanguageCode[]
		{
			LanguageCode.EN,
			LanguageCode.PT,
			LanguageCode.RU,
			LanguageCode.DE,
			LanguageCode.FR,
			LanguageCode.ES,
			LanguageCode.TR,
			LanguageCode.PL
		};
	}
}
