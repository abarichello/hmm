using System;
using HeavyMetalMachines.Localization;
using Hoplon.Localization.TranslationTable;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class GadgetStatInfo
	{
		public string LocalizedLabel
		{
			get
			{
				this.CheckTranslationContext();
				return Language.Get(this.Label, this._context);
			}
		}

		private void CheckTranslationContext()
		{
			if (this._context != TranslationContext.All)
			{
				return;
			}
			this._context = ((!this.Label.StartsWith("SPONSOR")) ? TranslationContext.CharactersMatchInfo : TranslationContext.Sponsors);
		}

		public string Label;

		public string UpgradeName;

		public GadgetStatKind Kind;

		public int AttibuteIndex;

		public int Index;

		private ContextTag _context = TranslationContext.All;
	}
}
