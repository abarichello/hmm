using System;
using System.Collections.Generic;
using FMod;
using HeavyMetalMachines.VFX.PlotKids.VoiceChat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Options
{
	public class AudioOptions : GameHubBehaviour
	{
		public float MusicVolume
		{
			get
			{
				return this._musicVolume;
			}
			set
			{
				if (value == this._musicVolume && !this.ForceValues)
				{
					return;
				}
				this._musicVolume = Mathf.Clamp(value, 0f, 2f);
				if (this.musicVolumeVCA != null)
				{
					this.musicVolumeVCA.Volume = this._musicVolume;
				}
				this.HasPendingChanges = true;
			}
		}

		public float SfxGameplayVolume
		{
			get
			{
				return this._sfxGameplayVolume;
			}
			set
			{
				if (Mathf.Approximately(value, this._sfxGameplayVolume) && !this.ForceValues)
				{
					return;
				}
				this._sfxGameplayVolume = Mathf.Clamp(value, 0f, 2f);
				if (this.sfxGameplayVolumeVCA != null)
				{
					this.sfxGameplayVolumeVCA.Volume = this._sfxGameplayVolume;
				}
				this.HasPendingChanges = true;
			}
		}

		public float SfxAmbientVolume
		{
			get
			{
				return this._sfxAmbientVolume;
			}
			set
			{
				if (value == this._sfxAmbientVolume && !this.ForceValues)
				{
					return;
				}
				this._sfxAmbientVolume = Mathf.Clamp(value, 0f, 2f);
				if (this.sfxAmbientVolumeVCA != null)
				{
					this.sfxAmbientVolumeVCA.Volume = this._sfxAmbientVolume;
				}
				this.HasPendingChanges = true;
			}
		}

		public float VoiceOverVolume
		{
			get
			{
				return this._voiceOverVolume;
			}
			set
			{
				if (value == this._voiceOverVolume && !this.ForceValues)
				{
					return;
				}
				this._voiceOverVolume = Mathf.Clamp(value, 0f, 2f);
				if (this.voiceOverVolumeVCA != null)
				{
					this.voiceOverVolumeVCA.Volume = this._voiceOverVolume;
				}
				this.HasPendingChanges = true;
			}
		}

		public float AnnouncerVolume
		{
			get
			{
				return this._announcerVolume;
			}
			set
			{
				if (value == this._announcerVolume && !this.ForceValues)
				{
					return;
				}
				this._announcerVolume = Mathf.Clamp(value, 0f, 2f);
				if (this.announcerVolumeVCA != null)
				{
					this.announcerVolumeVCA.Volume = this._announcerVolume;
				}
				this.HasPendingChanges = true;
			}
		}

		public float MasterVolume
		{
			get
			{
				return this._masterVolume;
			}
			set
			{
				if (Mathf.Approximately(this._masterVolume, value) && !this.ForceValues)
				{
					return;
				}
				this._masterVolume = Mathf.Clamp(value, 0f, 2f);
				if (this.masterVolumeVCA != null)
				{
					this.masterVolumeVCA.Volume = this._masterVolume;
				}
				if (this.OnMasterVolumeChanged != null)
				{
					this.OnMasterVolumeChanged(this._masterVolume);
				}
				this.HasPendingChanges = true;
			}
		}

		public float VoiceChatVolume
		{
			get
			{
				return this._voiceChatVolume;
			}
			set
			{
				if (Mathf.Approximately(value, this._voiceChatVolume) && !this.ForceValues)
				{
					return;
				}
				this._voiceChatVolume = Mathf.Clamp(value, 0f, 2f);
				if (this.OnVoiceChatVolumeChanged != null)
				{
					this.OnVoiceChatVolumeChanged(this._voiceChatVolume);
				}
				this.HasPendingChanges = true;
			}
		}

		public int AnnouncerIndex
		{
			get
			{
				return this._announcerIndex;
			}
			set
			{
				if (value == this._announcerIndex && !this.ForceValues)
				{
					return;
				}
				this._announcerIndex = Mathf.Clamp(value, 0, GameHubBehaviour.Hub.AudioSettings.AnnouncerVoiceOvers.Length - 1);
				if (this.OnAnnouncerIndexChanged != null)
				{
					this.OnAnnouncerIndexChanged();
				}
				this.HasPendingChanges = true;
			}
		}

		public int DriverIndex
		{
			get
			{
				return this._driverIndex;
			}
			set
			{
				if (value == this._driverIndex && !this.ForceValues)
				{
					return;
				}
				this._driverIndex = value;
				this.HasPendingChanges = true;
			}
		}

		public string[] DriverNames { get; set; }

		private void LoadPrefs()
		{
			this.ForceValues = true;
			this.DriverIndex = GameHubBehaviour.Hub.Config.GetIntSetting(AudioOptions.AudioOptionPrefs.OPTIONS_AUDIO_DRIVERINDEX.ToString(), this.DriverDefaultIndex);
			this.AnnouncerVolume = GameHubBehaviour.Hub.Config.GetFloatSetting(AudioOptions.AudioOptionPrefs.OPTIONS_AUDIO_ANNOUNCERVOLUME.ToString(), 1f);
			this.MasterVolume = GameHubBehaviour.Hub.Config.GetFloatSetting(AudioOptions.AudioOptionPrefs.OPTIONS_AUDIO_MASTERVOLUME.ToString(), 1f);
			this.SfxGameplayVolume = GameHubBehaviour.Hub.Config.GetFloatSetting(AudioOptions.AudioOptionPrefs.OPTIONS_AUDIO_SFXGAMEPLAYVOLUME.ToString(), 1f);
			this.MusicVolume = GameHubBehaviour.Hub.Config.GetFloatSetting(AudioOptions.AudioOptionPrefs.OPTIONS_AUDIO_MUSICVOLUME.ToString(), 1f);
			this.SfxAmbientVolume = GameHubBehaviour.Hub.Config.GetFloatSetting(AudioOptions.AudioOptionPrefs.OPTIONS_AUDIO_SFXAMBIENTVOLUME.ToString(), 1f);
			this.VoiceOverVolume = GameHubBehaviour.Hub.Config.GetFloatSetting(AudioOptions.AudioOptionPrefs.OPTIONS_AUDIO_VOICEOVERVOLUME.ToString(), 1f);
			this.VoiceChatVolume = GameHubBehaviour.Hub.Config.GetFloatSetting(AudioOptions.AudioOptionPrefs.OPTIONS_AUDIO_VOICECHATVOLUME.ToString(), 1f);
			this.AnnouncerIndex = GameHubBehaviour.Hub.Config.GetIntSetting(AudioOptions.AudioOptionPrefs.OPTIONS_AUDIO_ANNOUNCER_INDEX.ToString(), new Func<int>(this.GetDefaultAnnouncerIndex));
			this.ForceValues = false;
		}

		private int GetDefaultAnnouncerIndex()
		{
			LanguageCode languageCode = Language.CurrentLanguage();
			if (languageCode != LanguageCode.PT && languageCode != LanguageCode.PT_BR)
			{
				return 0;
			}
			return 1;
		}

		private void Refresh()
		{
			this._drivers = new List<AudioOptions.AudioDriver>();
			this._drivers.Add(new AudioOptions.AudioDriver
			{
				id = 0,
				name = "Default"
			});
			this.DriverNames = new string[this._drivers.Count];
			for (int i = 0; i < this._drivers.Count; i++)
			{
				this.DriverNames[i] = this._drivers[i].name;
			}
			if (this.musicVolumeVCA != null)
			{
				this._musicVolume = this.musicVolumeVCA.Volume;
			}
			if (this.sfxGameplayVolumeVCA != null)
			{
				this.SfxGameplayVolume = this.sfxGameplayVolumeVCA.Volume;
			}
			if (this.sfxAmbientVolumeVCA != null)
			{
				this.SfxAmbientVolume = this.sfxAmbientVolumeVCA.Volume;
			}
			if (this.voiceOverVolumeVCA != null)
			{
				this._voiceOverVolume = this.voiceOverVolumeVCA.Volume;
			}
			if (this.announcerVolumeVCA != null)
			{
				this._announcerVolume = this.announcerVolumeVCA.Volume;
			}
			this.HasPendingChanges = false;
		}

		public void Apply()
		{
			if (!this.HasPendingChanges)
			{
				return;
			}
			if (this.DriverIndex < 0 || this.DriverIndex >= this._drivers.Count)
			{
				this.DriverIndex = this.DriverDefaultIndex;
			}
			if (this.musicVolumeVCA != null)
			{
				this.musicVolumeVCA.Volume = this.MusicVolume;
			}
			if (this.sfxGameplayVolumeVCA != null)
			{
				this.sfxGameplayVolumeVCA.Volume = this.SfxGameplayVolume;
			}
			if (this.sfxAmbientVolumeVCA != null)
			{
				this.sfxAmbientVolumeVCA.Volume = this.SfxAmbientVolume;
			}
			if (this.voiceOverVolumeVCA != null)
			{
				this.voiceOverVolumeVCA.Volume = this.VoiceOverVolume;
			}
			if (this.masterVolumeVCA != null)
			{
				this.masterVolumeVCA.Volume = this.MasterVolume;
			}
			if (this.announcerVolumeVCA != null)
			{
				this.announcerVolumeVCA.Volume = this.AnnouncerVolume;
			}
			GameHubBehaviour.Hub.Config.SetIntSetting(AudioOptions.AudioOptionPrefs.OPTIONS_AUDIO_DRIVERINDEX.ToString(), this.DriverIndex);
			GameHubBehaviour.Hub.Config.SetFloatSetting(AudioOptions.AudioOptionPrefs.OPTIONS_AUDIO_ANNOUNCERVOLUME.ToString(), this.AnnouncerVolume);
			GameHubBehaviour.Hub.Config.SetFloatSetting(AudioOptions.AudioOptionPrefs.OPTIONS_AUDIO_MASTERVOLUME.ToString(), this.MasterVolume);
			GameHubBehaviour.Hub.Config.SetFloatSetting(AudioOptions.AudioOptionPrefs.OPTIONS_AUDIO_SFXGAMEPLAYVOLUME.ToString(), this.SfxGameplayVolume);
			GameHubBehaviour.Hub.Config.SetFloatSetting(AudioOptions.AudioOptionPrefs.OPTIONS_AUDIO_MUSICVOLUME.ToString(), this.MusicVolume);
			GameHubBehaviour.Hub.Config.SetFloatSetting(AudioOptions.AudioOptionPrefs.OPTIONS_AUDIO_SFXAMBIENTVOLUME.ToString(), this.SfxAmbientVolume);
			GameHubBehaviour.Hub.Config.SetFloatSetting(AudioOptions.AudioOptionPrefs.OPTIONS_AUDIO_VOICEOVERVOLUME.ToString(), this.VoiceOverVolume);
			GameHubBehaviour.Hub.Config.SetFloatSetting(AudioOptions.AudioOptionPrefs.OPTIONS_AUDIO_VOICECHATVOLUME.ToString(), this.VoiceChatVolume);
			GameHubBehaviour.Hub.Config.SetIntSetting(AudioOptions.AudioOptionPrefs.OPTIONS_AUDIO_ANNOUNCER_INDEX.ToString(), this.AnnouncerIndex);
			this.HasPendingChanges = false;
			GameHubBehaviour.Hub.Config.SaveSettings();
		}

		public void ResetDefault()
		{
			this.DriverIndex = this.DriverDefaultIndex;
			this.AnnouncerVolume = 1f;
			this.MasterVolume = 1f;
			this.SfxGameplayVolume = 1f;
			this.MusicVolume = 1f;
			this.SfxAmbientVolume = 1f;
			this.VoiceOverVolume = 1f;
			this.VoiceChatVolume = 1f;
			SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatInputType = VoiceChatInputType.Pressed;
			SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatTeamStatus = VoiceChatTeamStatus.Enable;
			this.AnnouncerIndex = this.GetDefaultAnnouncerIndex();
		}

		public void Cancel()
		{
			this.LoadPrefs();
			if (this.musicVolumeVCA != null)
			{
				this.musicVolumeVCA.Volume = this.MusicVolume;
			}
			if (this.sfxGameplayVolumeVCA != null)
			{
				this.sfxGameplayVolumeVCA.Volume = this.SfxGameplayVolume;
			}
			if (this.sfxAmbientVolumeVCA != null)
			{
				this.sfxAmbientVolumeVCA.Volume = this.SfxAmbientVolume;
			}
			if (this.voiceOverVolumeVCA != null)
			{
				this.voiceOverVolumeVCA.Volume = this.VoiceOverVolume;
			}
			if (this.announcerVolumeVCA != null)
			{
				this.announcerVolumeVCA.Volume = this.AnnouncerVolume;
			}
			if (this.masterVolumeVCA != null)
			{
				this.masterVolumeVCA.Volume = this.MasterVolume;
			}
			this.HasPendingChanges = false;
		}

		private void Start()
		{
			this.masterVolumeVCA = new FModVCAController(GameHubBehaviour.Hub.AudioSettings.masterVolumeVCAAsset);
			this.musicVolumeVCA = new FModVCAController(GameHubBehaviour.Hub.AudioSettings.musicVolumeVCAAsset);
			this.sfxGameplayVolumeVCA = new FModVCAController(GameHubBehaviour.Hub.AudioSettings.sfxGameplayVolumeVCAAsset);
			this.sfxAmbientVolumeVCA = new FModVCAController(GameHubBehaviour.Hub.AudioSettings.sfxAmbientVolumeVCAAsset);
			this.announcerVolumeVCA = new FModVCAController(GameHubBehaviour.Hub.AudioSettings.announcerVolumeVCAAsset);
			this.voiceOverVolumeVCA = new FModVCAController(GameHubBehaviour.Hub.AudioSettings.voiceOverVolumeVCAAsset);
			this.Refresh();
			this.LoadPrefs();
			this.Apply();
		}

		public bool HasPendingChanges;

		private bool ForceValues;

		private FModVCAController musicVolumeVCA;

		private float _musicVolume;

		private FModVCAController sfxGameplayVolumeVCA;

		private float _sfxGameplayVolume;

		private FModVCAController sfxAmbientVolumeVCA;

		private float _sfxAmbientVolume;

		private FModVCAController voiceOverVolumeVCA;

		private float _voiceOverVolume;

		private FModVCAController announcerVolumeVCA;

		private float _announcerVolume;

		private FModVCAController masterVolumeVCA;

		private float _masterVolume;

		public Action<float> OnMasterVolumeChanged;

		public float _voiceChatVolume = 1f;

		public Action<float> OnVoiceChatVolumeChanged;

		public int _announcerIndex;

		public Action OnAnnouncerIndexChanged;

		public const float VolumeMin = 0f;

		public const float VolumeMax = 2f;

		public const float VolumeDefault = 1f;

		private List<AudioOptions.AudioDriver> _drivers;

		private int _driverIndex;

		private readonly int DriverDefaultIndex;

		private class AudioDriver
		{
			public int id;

			public string name;
		}

		private enum AudioOptionPrefs
		{
			OPTIONS_AUDIO_ANNOUNCERVOLUME,
			OPTIONS_AUDIO_SFXGAMEPLAYVOLUME,
			OPTIONS_AUDIO_MUSICVOLUME,
			OPTIONS_AUDIO_SFXAMBIENTVOLUME,
			OPTIONS_AUDIO_VOICECHATVOLUME,
			OPTIONS_AUDIO_DRIVERINDEX,
			OPTIONS_AUDIO_LANGUAGEINDEX,
			OPTIONS_AUDIO_MAINMENUMUSIC,
			OPTIONS_AUDIO_INTERFACEFX,
			OPTIONS_AUDIO_VOICEOVERVOLUME,
			OPTIONS_AUDIO_MASTERVOLUME,
			OPTIONS_AUDIO_ANNOUNCER_INDEX
		}
	}
}
