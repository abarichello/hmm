using System;
using System.Collections.Generic;
using HeavyMetalMachines.VFX.PlotKids.VoiceChat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class EscMenuAudioGui : EscMenuScreenGui
	{
		public override void ReloadCurrent()
		{
			this.AudioAnnouncerSlider.Set(GameHubBehaviour.Hub.Options.Audio.AnnouncerVolume / 2f, false);
			this.AudioAnnouncerLabel.text = Mathf.RoundToInt(GameHubBehaviour.Hub.Options.Audio.AnnouncerVolume * 100f).ToString();
			this.AudioMusicSlider.Set(GameHubBehaviour.Hub.Options.Audio.MusicVolume / 2f, false);
			this.AudioMusicLabel.text = Mathf.RoundToInt(GameHubBehaviour.Hub.Options.Audio.MusicVolume * 100f).ToString();
			this.AudioSfxGameplaySlider.Set(GameHubBehaviour.Hub.Options.Audio.SfxGameplayVolume / 2f, false);
			this.AudioSfxGameplayLabel.text = Mathf.RoundToInt(GameHubBehaviour.Hub.Options.Audio.SfxGameplayVolume * 100f).ToString();
			this.AudioSfxAmbientSlider.Set(GameHubBehaviour.Hub.Options.Audio.SfxAmbientVolume / 2f, false);
			this.AudioSfxAmbientLabel.text = Mathf.RoundToInt(GameHubBehaviour.Hub.Options.Audio.SfxAmbientVolume * 100f).ToString();
			this.AudioVoiceOverSlider.Set(GameHubBehaviour.Hub.Options.Audio.VoiceOverVolume / 2f, false);
			this.AudioVoiceOverLabel.text = Mathf.RoundToInt(GameHubBehaviour.Hub.Options.Audio.VoiceOverVolume * 100f).ToString();
			this.AudioMasterSlider.Set(GameHubBehaviour.Hub.Options.Audio.MasterVolume / 2f, false);
			this.AudioMasterLabel.text = Mathf.RoundToInt(GameHubBehaviour.Hub.Options.Audio.MasterVolume * 100f).ToString();
			this.AudioVoiceChatSlider.Set(GameHubBehaviour.Hub.Options.Audio.VoiceChatVolume / 2f, false);
			this.AudioVoiceChatLabel.text = Mathf.RoundToInt(GameHubBehaviour.Hub.Options.Audio.VoiceChatVolume * 100f).ToString();
			this.AudioDriverPopup.items = new List<string>(GameHubBehaviour.Hub.Options.Audio.DriverNames);
			this.AudioDriverPopup.Set(this.AudioDriverPopup.items[GameHubBehaviour.Hub.Options.Audio.DriverIndex], false);
			this.ActivationModePopup.items = new List<string>
			{
				Language.Get("VoiceChatInputType_Pressed", TranslationSheets.Chat),
				Language.Get("VoiceChatInputType_Toggle", TranslationSheets.Chat),
				Language.Get("VoiceChatInputType_AlwaysActive", TranslationSheets.Chat)
			};
			this.ActivationModePopup.value = this.ActivationModePopup.items[(int)SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatInputType];
			this.ActivationVoiceTeamPopup.items = new List<string>
			{
				Language.Get("VoiceChatTeamStatus_Disable", TranslationSheets.Chat),
				Language.Get("VoiceChatTeamStatus_Enable", TranslationSheets.Chat)
			};
			this.ActivationVoiceTeamPopup.value = this.ActivationVoiceTeamPopup.items[(int)SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatTeamStatus];
			this.AnnouncerPopup.items.Clear();
			for (int i = 0; i < GameHubBehaviour.Hub.AudioSettings.AnnouncerVoiceOvers.Length; i++)
			{
				this.AnnouncerPopup.items.Add(Language.Get(GameHubBehaviour.Hub.AudioSettings.AnnouncerVoiceOvers[i].draftName, TranslationSheets.Options));
			}
			this.AnnouncerPopup.value = Language.Get(GameHubBehaviour.Hub.AudioSettings.AnnouncerVoiceOvers[GameHubBehaviour.Hub.Options.Audio.AnnouncerIndex].draftName, TranslationSheets.Options);
		}

		public override void ResetDefault()
		{
			GameHubBehaviour.Hub.Options.Audio.ResetDefault();
			GameHubBehaviour.Hub.Options.Audio.Apply();
		}

		public void OnAudioMasterSliderChanged()
		{
			GameHubBehaviour.Hub.Options.Audio.MasterVolume = this.AudioMasterSlider.value * 2f;
			GameHubBehaviour.Hub.Options.Audio.Apply();
			this.ReloadCurrent();
		}

		public void OnAudioAnnouncerSliderChanged()
		{
			GameHubBehaviour.Hub.Options.Audio.AnnouncerVolume = this.AudioAnnouncerSlider.value * 2f;
			GameHubBehaviour.Hub.Options.Audio.Apply();
			this.ReloadCurrent();
		}

		public void OnAudioMusicSliderChanged()
		{
			GameHubBehaviour.Hub.Options.Audio.MusicVolume = this.AudioMusicSlider.value * 2f;
			GameHubBehaviour.Hub.Options.Audio.Apply();
			this.ReloadCurrent();
		}

		public void OnAudioSfxGameplaySliderChanged()
		{
			GameHubBehaviour.Hub.Options.Audio.SfxGameplayVolume = this.AudioSfxGameplaySlider.value * 2f;
			GameHubBehaviour.Hub.Options.Audio.Apply();
			this.ReloadCurrent();
		}

		public void OnAudioSfxAmbientSliderChanged()
		{
			GameHubBehaviour.Hub.Options.Audio.SfxAmbientVolume = this.AudioSfxAmbientSlider.value * 2f;
			GameHubBehaviour.Hub.Options.Audio.Apply();
			this.ReloadCurrent();
		}

		public void OnVoiceOverSliderChanged()
		{
			GameHubBehaviour.Hub.Options.Audio.VoiceOverVolume = this.AudioVoiceOverSlider.value * 2f;
			GameHubBehaviour.Hub.Options.Audio.Apply();
			this.ReloadCurrent();
		}

		public void OnVoiceChatSliderChanged()
		{
			GameHubBehaviour.Hub.Options.Audio.VoiceChatVolume = this.AudioVoiceChatSlider.value * 2f;
			GameHubBehaviour.Hub.Options.Audio.Apply();
			this.ReloadCurrent();
		}

		public void OnAudioDriverPopupChanged()
		{
			GameHubBehaviour.Hub.Options.Audio.DriverIndex = this.AudioDriverPopup.items.FindIndex((string i) => i == this.AudioDriverPopup.value);
			GameHubBehaviour.Hub.Options.Audio.Apply();
			this.ReloadCurrent();
		}

		public void OnAnnoucerPopupChanged()
		{
			GameHubBehaviour.Hub.Options.Audio.AnnouncerIndex = this.AnnouncerPopup.items.FindIndex((string i) => i == this.AnnouncerPopup.value);
			GameHubBehaviour.Hub.Options.Audio.Apply();
			this.AnnouncerPopup.value = Language.Get(GameHubBehaviour.Hub.AudioSettings.AnnouncerVoiceOvers[GameHubBehaviour.Hub.Options.Audio.AnnouncerIndex].draftName, TranslationSheets.Options);
			this.ReloadCurrent();
		}

		public void OnAudioActivationModePopupChanged()
		{
			SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatInputType = (VoiceChatInputType)this.ActivationModePopup.items.FindIndex((string i) => i == this.ActivationModePopup.value);
			this.ReloadCurrent();
		}

		public void OnAudioActivationVoiceTeamPopupChanged()
		{
			SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatTeamStatus = (VoiceChatTeamStatus)this.ActivationVoiceTeamPopup.items.FindIndex((string i) => i == this.ActivationVoiceTeamPopup.value);
		}

		[Header("VOLUME")]
		public UISlider AudioMasterSlider;

		public UILabel AudioMasterLabel;

		public UISlider AudioMusicSlider;

		public UILabel AudioMusicLabel;

		public UISlider AudioSfxGameplaySlider;

		public UILabel AudioSfxGameplayLabel;

		public UISlider AudioSfxAmbientSlider;

		public UILabel AudioSfxAmbientLabel;

		public UISlider AudioAnnouncerSlider;

		public UILabel AudioAnnouncerLabel;

		public UISlider AudioVoiceOverSlider;

		public UILabel AudioVoiceOverLabel;

		public UISlider AudioVoiceChatSlider;

		public UILabel AudioVoiceChatLabel;

		[Header("Voice Chat")]
		public UIPopupList AnnouncerPopup;

		[Header("ADVANCED")]
		public UIPopupList AudioDriverPopup;

		public UIToggle MainMenuThemeCheckBox;

		[Header("Voice Chat")]
		public UIPopupList ActivationModePopup;

		public UIPopupList ActivationVoiceTeamPopup;
	}
}
