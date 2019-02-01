using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	[AddComponentMenu("UI/HMM/HmmUiText")]
	public class HmmUiText : Text
	{
		public override string text
		{
			get
			{
				return base.text;
			}
			set
			{
				switch (this.TextStyle)
				{
				case HmmUiText.TextStyles.Default:
					base.text = value;
					break;
				case HmmUiText.TextStyles.TitleCase:
					base.text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(CultureInfo.CurrentCulture.TextInfo.ToLower(value));
					break;
				case HmmUiText.TextStyles.UpperCase:
					base.text = CultureInfo.CurrentCulture.TextInfo.ToUpper(value);
					break;
				case HmmUiText.TextStyles.LowerCase:
					base.text = CultureInfo.CurrentCulture.TextInfo.ToLower(value);
					break;
				}
			}
		}

		protected override void Start()
		{
			base.Start();
			if (this.UseTranslation && Application.isPlaying)
			{
				this.ApplyTranslationText();
			}
		}

		public void ApplyTranslationText()
		{
			this.text = ((this.TranslationSheet != TranslationSheets.All) ? Language.Get(this.TranslationDraft, this.TranslationSheet) : Language.Get(this.TranslationDraft));
		}

		public HmmUiText.TextStyles TextStyle;

		public bool UseTranslation;

		public TranslationSheets TranslationSheet;

		public string TranslationDraft;

		public enum TextStyles
		{
			Default,
			TitleCase,
			UpperCase,
			LowerCase
		}
	}
}
