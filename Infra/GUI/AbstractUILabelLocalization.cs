using System;
using System.Globalization;
using HeavyMetalMachines.Localization;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Infra.GUI
{
	[RequireComponent(typeof(UILabel))]
	public abstract class AbstractUILabelLocalization : GameHubBehaviour
	{
		public abstract string Draft { get; }

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
			string text = Language.Get(this.Draft, this.sheet);
			AbstractUILabelLocalization.TextStyles textStyles = this.textStyle;
			if (textStyles != AbstractUILabelLocalization.TextStyles.LowerCase)
			{
				if (textStyles != AbstractUILabelLocalization.TextStyles.TitleCase)
				{
					if (textStyles == AbstractUILabelLocalization.TextStyles.UpperCase)
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

		public AbstractUILabelLocalization.TextStyles textStyle;

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
