﻿using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.UnityUI
{
	public class UnityUiTimerInfo : MonoBehaviour
	{
		public void Setup(TimeSpan remainingTime, TimeSpan warningTime, string formatedText = "<color=#{0}>{1}</color>")
		{
			string text;
			if (remainingTime.Days > 0)
			{
				text = string.Format("{0} {1}", remainingTime.Days, Language.Get("GUI_TIME_DAYS", TranslationContext.GUI));
			}
			else if (remainingTime.Hours > 0)
			{
				text = string.Format("{0} {1}", remainingTime.Hours, Language.Get("GUI_TIME_HOURS", TranslationContext.GUI));
			}
			else if (remainingTime.Minutes > 0)
			{
				text = string.Format("{0} {1}", remainingTime.Minutes, Language.Get("GUI_TIME_MINUTES", TranslationContext.GUI));
			}
			else
			{
				text = Language.Get("GUI_TIME_LESS_THEN_A_MINUTE", TranslationContext.GUI);
			}
			if (remainingTime > warningTime)
			{
				this._clockInfoRawImage.sprite = this._clockTexture;
				this._borderImage.color = this._normalBorderColor;
				this._borderImage.SetAlpha(0f);
				this._timeText.text = Language.Format(formatedText, new object[]
				{
					HudUtils.RGBToHex(this._normalTextColor),
					text
				});
			}
			else
			{
				this._clockInfoRawImage.sprite = this._warningTexture;
				this._borderImage.sprite = this._warningBorderSprite;
				this._borderImage.color = this._warningBorderColor;
				this._borderImage.SetAlpha(1f);
				this._timeText.text = Language.Format(formatedText, new object[]
				{
					HudUtils.RGBToHex(this._warningTextColor),
					text
				});
			}
		}

		[SerializeField]
		private HmmUiImage _clockInfoRawImage;

		[SerializeField]
		private Image _borderImage;

		[SerializeField]
		private Text _timeText;

		[Header("[Colors]")]
		[SerializeField]
		private Color _normalTextColor;

		[SerializeField]
		private Color _warningTextColor;

		[SerializeField]
		private Color _normalBorderColor;

		[SerializeField]
		private Color _warningBorderColor;

		[Header("[Assets]")]
		[SerializeField]
		private Sprite _clockTexture;

		[SerializeField]
		private Sprite _warningTexture;

		[SerializeField]
		private Sprite _warningBorderSprite;
	}
}
