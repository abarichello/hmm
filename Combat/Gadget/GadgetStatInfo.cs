using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class GadgetStatInfo
	{
		public string LocalizedLabel
		{
			get
			{
				this.CheckTranslationSheet();
				return Language.Get(this.Label, this._translationSheet);
			}
		}

		private void CheckTranslationSheet()
		{
			if (this._translationSheet != TranslationSheets.All)
			{
				return;
			}
			this._translationSheet = ((!this.Label.StartsWith("SPONSOR")) ? TranslationSheets.CharactersMatchInfo : TranslationSheets.Sponsors);
		}

		public string Label;

		public string UpgradeName;

		public GadgetStatKind Kind;

		public int AttibuteIndex;

		public int Index;

		private TranslationSheets _translationSheet;
	}
}
