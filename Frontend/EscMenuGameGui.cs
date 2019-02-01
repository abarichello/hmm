using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class EscMenuGameGui : EscMenuScreenGui
	{
		public override void ReloadCurrent()
		{
			this.GameLanguagePopup.items = new List<string>(GameHubBehaviour.Hub.Options.Game.LanguageNames);
			this.GameLanguagePopup.value = this.GameLanguagePopup.items[GameHubBehaviour.Hub.Options.Game.LanguageIndex];
			this.ShowGadgetsLifebarToggle.Set(GameHubBehaviour.Hub.Options.Game.ShowGadgetsLifebar, false);
			this.ShowGadgetsCursorToggle.Set(GameHubBehaviour.Hub.Options.Game.ShowGadgetsCursor, false);
			this.ShowPingToggle.Set(GameHubBehaviour.Hub.Options.Game.ShowPing, false);
			this.CounselorActivationToggle.Set(GameHubBehaviour.Hub.Options.Game.CounselorActive, false);
			this.CounselorHudHintToggle.Set(GameHubBehaviour.Hub.Options.Game.CounselorHudHint, false);
			this.ShowLifebarTextToggle.Set(GameHubBehaviour.Hub.Options.Game.ShowLifebarText, false);
			this.ShowPlayerIndicator.Set(GameHubBehaviour.Hub.Options.Game.ShowPlayerIndicator, false);
			this.PlayerIndicatorAlphaSlider.Set(GameHubBehaviour.Hub.Options.Game.PlayerIndicatorAlpha, false);
			this.UpdateLabelPlayerIndicatorAlpha();
		}

		public override void ResetDefault()
		{
			GameHubBehaviour.Hub.Options.Game.ResetDefault();
			GameHubBehaviour.Hub.Options.Game.Apply();
			this.ReloadCurrent();
		}

		public void OnGameLanguagePopupChanged()
		{
			int num = this.GameLanguagePopup.items.FindIndex((string i) => i == this.GameLanguagePopup.value);
			if (GameHubBehaviour.Hub.Options.Game.LanguageIndex == num)
			{
				return;
			}
			GameHubBehaviour.Hub.Options.Game.LanguageIndex = num;
			if (GameHubBehaviour.Hub.Options.Game.HasPendingChanges)
			{
				Guid confirmWindowGuid = Guid.NewGuid();
				LanguageCode languageCode = GameHubBehaviour.Hub.Options.Game.LanguageIndexToCode(num);
				ConfirmWindowProperties properties = new ConfirmWindowProperties
				{
					Guid = confirmWindowGuid,
					QuestionText = Language.GetFromLanguage("GAME_LANGUAGE_WARNING_REBOOTREQUIRED", TranslationSheets.Options, languageCode),
					OkButtonText = Language.Get("GAME_LANGUAGE_WARNING_OK", TranslationSheets.Options),
					OnOk = delegate()
					{
						GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
					}
				};
				GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
			}
			GameHubBehaviour.Hub.Options.Game.Apply();
		}

		public void OnShowGadgetsLifebarChanged()
		{
			GameHubBehaviour.Hub.Options.Game.ShowGadgetsLifebar = this.ShowGadgetsLifebarToggle.value;
			GameHubBehaviour.Hub.Options.Game.Apply();
		}

		public void OnShowGadgetsCursorChanged()
		{
			GameHubBehaviour.Hub.Options.Game.ShowGadgetsCursor = this.ShowGadgetsCursorToggle.value;
			GameHubBehaviour.Hub.Options.Game.Apply();
		}

		public void OnShowPingChanged()
		{
			GameHubBehaviour.Hub.Options.Game.ShowPing = this.ShowPingToggle.value;
			GameHubBehaviour.Hub.Options.Game.Apply();
		}

		public void OnGameCounselorActivationChanged()
		{
			GameHubBehaviour.Hub.Options.Game.CounselorActive = this.CounselorActivationToggle.value;
			GameHubBehaviour.Hub.Options.Game.Apply();
			this.ReloadCurrent();
		}

		public void OnCounselorHudHintChanged()
		{
			GameHubBehaviour.Hub.Options.Game.CounselorHudHint = this.CounselorHudHintToggle.value;
			GameHubBehaviour.Hub.Options.Game.Apply();
		}

		public void OnShowLifebarTextChanged()
		{
			GameHubBehaviour.Hub.Options.Game.ShowLifebarText = this.ShowLifebarTextToggle.value;
			GameHubBehaviour.Hub.Options.Game.Apply();
		}

		public void OnShowPlayerIndicatorChanged()
		{
			GameHubBehaviour.Hub.Options.Game.ShowPlayerIndicator = this.ShowPlayerIndicator.value;
			GameHubBehaviour.Hub.Options.Game.Apply();
		}

		public void OnPlayerIndicatorAlphaSliderChanged()
		{
			GameHubBehaviour.Hub.Options.Game.PlayerIndicatorAlpha = this.PlayerIndicatorAlphaSlider.value;
			GameHubBehaviour.Hub.Options.Game.Apply();
			this.UpdateLabelPlayerIndicatorAlpha();
		}

		private void UpdateLabelPlayerIndicatorAlpha()
		{
			int num = 90;
			float num2 = GameHubBehaviour.Hub.Options.Game.PlayerIndicatorAlpha * (float)num + 10f;
			this.PlayerIndicatorAlphaLabel.text = num2.ToString("0");
		}

		[Header("[Language]")]
		public UIPopupList GameLanguagePopup;

		[Header("[Counselor]")]
		[SerializeField]
		private UIToggle CounselorActivationToggle;

		[SerializeField]
		private UIToggle CounselorHudHintToggle;

		[Header("[Gadget Feedbacks]")]
		[SerializeField]
		private UIToggle ShowGadgetsLifebarToggle;

		[SerializeField]
		private UIToggle ShowGadgetsCursorToggle;

		[Header("[Ping]")]
		[SerializeField]
		private UIToggle ShowPingToggle;

		[Header("[Lifebar]")]
		[SerializeField]
		private UIToggle ShowLifebarTextToggle;

		[Header("[Player Indicator]")]
		[SerializeField]
		private UIToggle ShowPlayerIndicator;

		[SerializeField]
		private UISlider PlayerIndicatorAlphaSlider;

		[SerializeField]
		private UILabel PlayerIndicatorAlphaLabel;

		public const int MAX_ALPHA_PLAYERINDICATOR_GUI = 100;

		public const int MIN_ALPHA_PLAYERINDICATOR_GUI = 10;
	}
}
