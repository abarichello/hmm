using System;
using HeavyMetalMachines.Localization;
using Pocketverse;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	public class HudPingController : GameHubBehaviour
	{
		protected void Awake()
		{
			this._mainCanvasGroup.alpha = ((!this.IsEnabledInOptions()) ? 0f : 1f);
			GameHubBehaviour.Hub.Options.Game.ListenToShowPingChanged += this.GameOnListenToShowPingChanged;
			string text = Language.Get(this._titleTextDraft, this._titleTextTranslationSheet);
			HoplonUiUtils.SetPreferedWidth(text, this._titleText, 0f);
			this._titleText.text = text;
			this.SetPingInfo((int)GameHubBehaviour.Hub.Net.GetPing());
			this._updateFrequencyCounterInSec = this.UpdateFrequencyInSec;
			this._testPingInMillis = 0;
		}

		protected void OnDestroy()
		{
			GameHubBehaviour.Hub.Options.Game.ListenToShowPingChanged -= this.GameOnListenToShowPingChanged;
		}

		private void GameOnListenToShowPingChanged(bool isVisible)
		{
			this._mainCanvasGroup.alpha = ((!isVisible) ? 0f : 1f);
		}

		private bool IsEnabledInOptions()
		{
			return GameHubBehaviour.Hub.Options.Game.ShowPing;
		}

		protected void Update()
		{
			if (this._mainCanvasGroup.alpha < 0.001f)
			{
				return;
			}
			if (this._updateFrequencyCounterInSec > 0f)
			{
				this._updateFrequencyCounterInSec -= Time.deltaTime;
				return;
			}
			this._updateFrequencyCounterInSec = this.UpdateFrequencyInSec;
			int num = (int)GameHubBehaviour.Hub.Net.GetPing();
			if (num != this._lastPingInMillis)
			{
				this.SetPingInfo(num);
			}
		}

		private void SetPingInfo(int pingInMillis)
		{
			if (!this.IsValidPing(pingInMillis))
			{
				this._pingText.text = "-";
				this.SetColors(this.InvalidPingInfo);
				this._lastPingInMillis = pingInMillis;
				return;
			}
			this._pingText.text = pingInMillis.ToString("0");
			if (pingInMillis > GameHubBehaviour.Hub.GuiScripts.GUIValues.HighPing)
			{
				this.SetColors(this.HighPingInfo);
			}
			else if (pingInMillis > GameHubBehaviour.Hub.GuiScripts.GUIValues.MediumPing)
			{
				this.SetColors(this.MediumPingInfo);
			}
			else
			{
				this.SetColors(this.LowPingInfo);
			}
			this._lastPingInMillis = pingInMillis;
		}

		private bool IsValidPing(int pingInMillis)
		{
			return pingInMillis > -1;
		}

		private void SetColors(HudPingController.HudPingInfo pingGradientColors)
		{
			this._pingImage.sprite = pingGradientColors.IconSprite;
			this._titleTextGradient.SetColors(pingGradientColors.GradientTopColor, pingGradientColors.GradientBottomColor);
			this._pingTextGradient.SetColors(pingGradientColors.GradientTopColor, pingGradientColors.GradientBottomColor);
		}

		[SerializeField]
		private CanvasGroup _mainCanvasGroup;

		[Header("[Icon]")]
		[SerializeField]
		private Image _pingImage;

		[Header("[Title]")]
		[SerializeField]
		private Text _titleText;

		[SerializeField]
		private HmmUiGradient _titleTextGradient;

		[SerializeField]
		private TranslationSheets _titleTextTranslationSheet;

		[SerializeField]
		private string _titleTextDraft;

		[Header("[Ping]")]
		[SerializeField]
		private Text _pingText;

		[SerializeField]
		private HmmUiGradient _pingTextGradient;

		[Header("[Info]")]
		[SerializeField]
		private HudPingController.HudPingInfo LowPingInfo;

		[SerializeField]
		private HudPingController.HudPingInfo MediumPingInfo;

		[SerializeField]
		private HudPingController.HudPingInfo HighPingInfo;

		[SerializeField]
		private HudPingController.HudPingInfo InvalidPingInfo;

		[Header("[Update frequency]")]
		[SerializeField]
		private float UpdateFrequencyInSec;

		[Header("[Test Only]")]
		[SerializeField]
		[Range(0f, 2000f)]
		private int _testPingInMillis;

		private int _lastPingInMillis;

		private float _updateFrequencyCounterInSec;

		[Serializable]
		private struct HudPingInfo
		{
			public Sprite IconSprite;

			public Color GradientTopColor;

			public Color GradientBottomColor;
		}
	}
}
