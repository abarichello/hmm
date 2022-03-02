using System;
using System.Collections.Generic;
using System.Linq;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.ParentalControl.Restrictions;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.VoiceChat.Business;
using Hoplon.Audio.Model;
using Hoplon.Input.UiNavigation.AxisSelector;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class EscMenuAudioGui : EscMenuScreenGui
	{
		private IUiNavigationAxisSelector UiNavigationAxisSelector
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

		public override void Show()
		{
			this.Dispose();
			this.CheckParentalControlRestriction();
			this.CheckToShowVoiceChatDevices();
			base.Show();
			this._scrollView.ResetPosition();
			this.ConfigureAudioDevices();
		}

		public override void Hide()
		{
			base.Hide();
			this.UiNavigationAxisSelector.ClearSelection();
			this.Dispose();
		}

		private void CheckToShowVoiceChatDevices()
		{
			bool active = this.ShouldGetVoiceChatDevice();
			this._inputDevicesGameObject.SetActive(active);
			this._inputDevicesLoadingGameObject.SetActive(active);
		}

		private bool ShouldGetVoiceChatDevice()
		{
			return this._shouldGetVoiceChatDevices.Should();
		}

		private void CheckParentalControlRestriction()
		{
			IVoiceChatRestriction voiceChatRestriction = this._diContainer.Resolve<IVoiceChatRestriction>();
			if (!voiceChatRestriction.IsGloballyEnabled())
			{
				this._voiceChatParentalControlFeedback.SetActive(false);
				return;
			}
			this.DisableVoiceChatSlider(this.AudioVoiceOverSlider);
			this.DisableVoiceChatSlider(this.AudioVoiceChatSlider);
			this.DisableVoiceChatDropBox(this.ActivationModePopup);
			this.DisableVoiceChatDropBox(this.ActivationVoiceTeamPopup);
			this._voiceChatParentalControlFeedback.SetActive(true);
		}

		private void DisableVoiceChatSlider(UISlider slider)
		{
			foreach (Collider collider in slider.GetComponentsInChildren<Collider>())
			{
				if (collider)
				{
					collider.enabled = false;
				}
			}
		}

		private void DisableVoiceChatDropBox(UIPopupList popupList)
		{
			Collider component = popupList.GetComponent<Collider>();
			if (component)
			{
				component.enabled = false;
			}
		}

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
			this.ActivationModePopup.items = new List<string>
			{
				Language.Get("VoiceChatInputType_Pressed", TranslationContext.Chat),
				Language.Get("VoiceChatInputType_Toggle", TranslationContext.Chat),
				Language.Get("VoiceChatInputType_AlwaysActive", TranslationContext.Chat)
			};
			this.ActivationModePopup.value = this.ActivationModePopup.items[this._voiceChatPreferences.InputType];
			this.ActivationVoiceTeamPopup.items = new List<string>
			{
				Language.Get("VoiceChatTeamStatus_Disable", TranslationContext.Chat),
				Language.Get("VoiceChatTeamStatus_Enable", TranslationContext.Chat)
			};
			this.ActivationVoiceTeamPopup.value = this.ActivationVoiceTeamPopup.items[this._voiceChatPreferences.TeamStatus];
			this.AnnouncerPopup.items.Clear();
			for (int i = 0; i < GameHubBehaviour.Hub.AudioSettings.AnnouncerVoiceOvers.Length; i++)
			{
				this.AnnouncerPopup.items.Add(Language.Get(GameHubBehaviour.Hub.AudioSettings.AnnouncerVoiceOvers[i].draftName, TranslationContext.Options));
			}
			this.AnnouncerPopup.value = Language.Get(GameHubBehaviour.Hub.AudioSettings.AnnouncerVoiceOvers[GameHubBehaviour.Hub.Options.Audio.AnnouncerIndex].draftName, TranslationContext.Options);
		}

		private void Dispose()
		{
			if (this._disposable != null)
			{
				this._disposable.Dispose();
			}
		}

		private void ConfigureAudioDevices()
		{
			if (!this.ShouldGetVoiceChatDevice())
			{
				return;
			}
			this._disposable = ObservableExtensions.Subscribe<Unit>(this.LoadThenShowAudioDevices());
		}

		private IObservable<Unit> LoadThenShowAudioDevices()
		{
			return Observable.AsUnitObservable<Unit>(Observable.Defer<Unit>(delegate()
			{
				this.ClearDevicesPopupAndShowLoading();
				return Observable.SelectMany<Unit, Unit>(this._voiceChatDevices.CheckAndDoVoiceLogin(), Observable.Merge<Unit>(this.ConfigureInputDevices(), new IObservable<Unit>[]
				{
					this.ConfigureOutputDevices()
				}));
			}));
		}

		private IObservable<Unit> ConfigureInputDevices()
		{
			return Observable.SelectMany<string, Unit>(Observable.SelectMany<DeviceModel, string>(Observable.Do<DeviceModel>(Observable.Do<DeviceModel>(Observable.SelectMany<DeviceModel[], DeviceModel>(Observable.Do<DeviceModel[]>(Observable.Do<DeviceModel[]>(this._voiceChatDevices.GetInputDevices(), delegate(DeviceModel[] _)
			{
				this.ShowInputLoadingAndHideDevicePopup();
			}), new Action<DeviceModel[]>(this.AddInputPopupOptions)), this._voiceChatDevices.GetCurrentInputDevice()), delegate(DeviceModel currentDevice)
			{
				this._inputDevicesPopup.SelectedLabel = currentDevice.DisplayName;
			}), delegate(DeviceModel _)
			{
				this.HideInputLoadingAndShowDevicePopup();
			}), (DeviceModel _) => this._inputDevicesPopup.OnSelectionChanged()), (string device) => this.SaveInputDevice(device));
		}

		private IObservable<Unit> ConfigureOutputDevices()
		{
			return Observable.SelectMany<string, Unit>(Observable.SelectMany<DeviceModel, string>(Observable.Do<DeviceModel>(Observable.Do<DeviceModel>(Observable.SelectMany<DeviceModel[], DeviceModel>(Observable.Do<DeviceModel[]>(Observable.Do<DeviceModel[]>(this._voiceChatDevices.GetOutputDevices(), delegate(DeviceModel[] _)
			{
				this.ShowOutputLoadingAndHideDevicePopup();
			}), new Action<DeviceModel[]>(this.AddOutputPopupOptions)), this._voiceChatDevices.GetCurrentOutputDevice()), delegate(DeviceModel currentDevice)
			{
				this._outputDevicesPopup.SelectedLabel = currentDevice.DisplayName;
			}), delegate(DeviceModel _)
			{
				this.HideOutputLoadingAndShowDevicePopup();
			}), (DeviceModel _) => this._outputDevicesPopup.OnSelectionChanged()), (string device) => this.SaveOutputDevice(device));
		}

		private IObservable<Unit> SaveInputDevice(string deviceId)
		{
			return Observable.Defer<Unit>(delegate()
			{
				DeviceModel inputDevice = this._voiceInputDevices.FirstOrDefault((DeviceModel d) => d.DeviceId == deviceId);
				return this._voiceChatDevices.SetInputDevice(inputDevice);
			});
		}

		private IObservable<Unit> SaveOutputDevice(string deviceId)
		{
			return Observable.Defer<Unit>(delegate()
			{
				DeviceModel outputDevice = this._voiceOutputDevices.FirstOrDefault((DeviceModel d) => d.DeviceId == deviceId);
				return this._voiceChatDevices.SetOutputDevice(outputDevice);
			});
		}

		private void HideInputLoadingAndShowDevicePopup()
		{
			this._inputDevicesPopup.Interactable = true;
			this._inputDevicesLoadingGameObject.SetActive(false);
		}

		private void HideOutputLoadingAndShowDevicePopup()
		{
			this._outputDevicesPopup.Interactable = true;
			this._outputDevicesLoadingGameObject.SetActive(false);
		}

		private void ShowInputLoadingAndHideDevicePopup()
		{
			this._inputDevicesPopup.Hide();
			this._inputDevicesPopup.ClearOptions();
			this._inputDevicesPopup.Interactable = false;
			this._inputDevicesLoadingGameObject.SetActive(true);
		}

		private void ShowOutputLoadingAndHideDevicePopup()
		{
			this._outputDevicesPopup.Hide();
			this._outputDevicesPopup.ClearOptions();
			this._outputDevicesPopup.Interactable = false;
			this._outputDevicesLoadingGameObject.SetActive(true);
		}

		private void ClearDevicesPopupAndShowLoading()
		{
			this.ShowOutputLoadingAndHideDevicePopup();
			this.ShowInputLoadingAndHideDevicePopup();
		}

		private void AddInputPopupOptions(DeviceModel[] activeDevices)
		{
			this._voiceInputDevices = activeDevices;
			List<string> labels = (from device in activeDevices
			select device.DisplayName).ToList<string>();
			List<string> options = (from device in activeDevices
			select device.DeviceId).ToList<string>();
			this._inputDevicesPopup.AddOptions(options, labels);
		}

		private void AddOutputPopupOptions(DeviceModel[] activeDevices)
		{
			this._voiceOutputDevices = activeDevices;
			List<string> labels = (from device in activeDevices
			select device.DisplayName).ToList<string>();
			List<string> options = (from device in activeDevices
			select device.DeviceId).ToList<string>();
			this._outputDevicesPopup.AddOptions(options, labels);
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

		public void OnAnnoucerPopupChanged()
		{
			GameHubBehaviour.Hub.Options.Audio.AnnouncerIndex = this.AnnouncerPopup.items.FindIndex((string i) => i == this.AnnouncerPopup.value);
			GameHubBehaviour.Hub.Options.Audio.Apply();
			this.AnnouncerPopup.value = Language.Get(GameHubBehaviour.Hub.AudioSettings.AnnouncerVoiceOvers[GameHubBehaviour.Hub.Options.Audio.AnnouncerIndex].draftName, TranslationContext.Options);
			this.ReloadCurrent();
		}

		public void OnAudioActivationModePopupChanged()
		{
			this._voiceChatPreferences.InputType = this.ActivationModePopup.items.FindIndex((string i) => i == this.ActivationModePopup.value);
			this.ReloadCurrent();
		}

		public void OnAudioActivationVoiceTeamPopupChanged()
		{
			this._voiceChatPreferences.TeamStatus = this.ActivationVoiceTeamPopup.items.FindIndex((string i) => i == this.ActivationVoiceTeamPopup.value);
		}

		[Header("[VOLUME]")]
		[SerializeField]
		private UISlider AudioMasterSlider;

		[SerializeField]
		private UILabel AudioMasterLabel;

		[SerializeField]
		private UISlider AudioMusicSlider;

		[SerializeField]
		private UILabel AudioMusicLabel;

		[SerializeField]
		private UISlider AudioSfxGameplaySlider;

		[SerializeField]
		private UILabel AudioSfxGameplayLabel;

		[SerializeField]
		private UISlider AudioSfxAmbientSlider;

		[SerializeField]
		private UILabel AudioSfxAmbientLabel;

		[SerializeField]
		private UISlider AudioAnnouncerSlider;

		[SerializeField]
		private UILabel AudioAnnouncerLabel;

		[SerializeField]
		private UISlider AudioVoiceOverSlider;

		[SerializeField]
		private UILabel AudioVoiceOverLabel;

		[SerializeField]
		private UISlider AudioVoiceChatSlider;

		[SerializeField]
		private UILabel AudioVoiceChatLabel;

		[Header("[Announcer]")]
		[SerializeField]
		private UIPopupList AnnouncerPopup;

		[Header("[Voice Chat]")]
		[SerializeField]
		private UIPopupList ActivationModePopup;

		[SerializeField]
		private UIPopupList ActivationVoiceTeamPopup;

		[SerializeField]
		private StringNGuiDropdown _inputDevicesPopup;

		[SerializeField]
		private StringNGuiDropdown _outputDevicesPopup;

		[SerializeField]
		private GameObject _voiceChatParentalControlFeedback;

		[SerializeField]
		private GameObject _inputDevicesGameObject;

		[SerializeField]
		private GameObject _inputDevicesLoadingGameObject;

		[SerializeField]
		private GameObject _outputDevicesLoadingGameObject;

		[Header("[Scroll]")]
		[SerializeField]
		private UIScrollView _scrollView;

		[Header("[Ui Navigation]")]
		[SerializeField]
		private UiNavigationAxisSelector _uiNavigationAxisSelector;

		[Inject]
		private IVoiceChatPreferences _voiceChatPreferences;

		[Inject]
		private IShouldGetVoiceChatDevices _shouldGetVoiceChatDevices;

		[Inject]
		private DiContainer _diContainer;

		[Inject]
		private IVoiceChatDevicesFacade _voiceChatDevices;

		private DeviceModel[] _voiceInputDevices;

		private DeviceModel[] _voiceOutputDevices;

		private IDisposable _disposable;
	}
}
