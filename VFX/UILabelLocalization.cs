using System;
using System.Globalization;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[RequireComponent(typeof(UILabel))]
	public class UILabelLocalization : GameHubBehaviour
	{
		private void Start()
		{
			this._label = base.GetComponent<UILabel>();
			this.ApplyText();
		}

		private void ApplyText()
		{
			if (GameHubBehaviour.Hub == null || GameHubBehaviour.Hub.Net == null)
			{
				return;
			}
			string text = (this.sheet != TranslationSheets.All) ? Language.Get(this.draft, this.sheet.ToString()) : Language.Get(this.draft);
			UILabelLocalization.TextStyles textStyles = this.textStyle;
			if (textStyles != UILabelLocalization.TextStyles.LowerCase)
			{
				if (textStyles != UILabelLocalization.TextStyles.TitleCase)
				{
					if (textStyles == UILabelLocalization.TextStyles.UpperCase)
					{
						text = CultureInfo.CurrentCulture.TextInfo.ToUpper(text);
					}
				}
				else
				{
					text = CultureInfo.CurrentCulture.TextInfo.ToLower(text);
					text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text);
				}
			}
			else
			{
				text = CultureInfo.CurrentCulture.TextInfo.ToLower(text);
			}
			this._label.text = text;
		}

		public TranslationSheets sheet;

		public string draft;

		public UILabelLocalization.TextStyles textStyle;

		private UILabel _label;

		public enum TextStyles
		{
			Default,
			TitleCase,
			UpperCase,
			LowerCase
		}
	}
}
