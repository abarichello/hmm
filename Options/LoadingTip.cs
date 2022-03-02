using System;
using HeavyMetalMachines.Localization;
using Standard_Assets.Scripts.HMM.Util;

namespace HeavyMetalMachines.Options
{
	[Serializable]
	public class LoadingTip
	{
		public TranslationSheets TranslationSheet;

		public MultiPlatformLocalizationDraft TranslationDraft;

		public string BackgroundSpriteName;
	}
}
