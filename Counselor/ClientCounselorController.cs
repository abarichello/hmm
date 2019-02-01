using System;
using System.Diagnostics;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using Assets.Standard_Assets.Scripts.Infra.KeyBoardLayout;
using FMod;
using HeavyMetalMachines.Infra.Counselor;
using HeavyMetalMachines.Options;
using Pocketverse;

namespace HeavyMetalMachines.Counselor
{
	public class ClientCounselorController : GameHubBehaviour
	{
		public CounselorConfig.AdvicesConfig CurrentAdviceConfig
		{
			get
			{
				return this._adviceStatus[this._currentAdviceConfigIndex].config;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnAudioPlayingChanged;

		public bool IsPlaying
		{
			get
			{
				return this._isPlayingAudio;
			}
		}

		private bool IsAudioValid
		{
			get
			{
				return this._currentPlayingAudio != null && !this._currentPlayingAudio.IsInvalidated();
			}
		}

		private void OnEnable()
		{
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.State_ListenToStateChanged;
			GameHubBehaviour.Hub.Options.Game.OnCounselorActiveChanged += this.GameOnOnCounselorActiveChanged;
			this._adviceStatus = new ClientCounselorController.AdviceStatus[GameHubBehaviour.Hub.CounselorConfig.Advices.Length];
			for (int i = 0; i < this._adviceStatus.Length; i++)
			{
				this._adviceStatus[i].config = GameHubBehaviour.Hub.CounselorConfig.Advices[i];
			}
			this.ConfigureDependencies();
		}

		private void ConfigureDependencies()
		{
			for (int i = 0; i < this._adviceStatus.Length; i++)
			{
				if (string.IsNullOrEmpty(this._adviceStatus[i].config.DependencyAdviceName))
				{
					this._adviceStatus[i].dependencyIndex = -1;
				}
				else
				{
					for (int j = 0; j < this._adviceStatus.Length; j++)
					{
						if (string.Equals(this._adviceStatus[i].config.DependencyAdviceName, this._adviceStatus[j].config.TranslationKey, StringComparison.Ordinal))
						{
							this._adviceStatus[i].dependencyIndex = j;
							break;
						}
					}
				}
			}
		}

		private void GameOnOnCounselorActiveChanged()
		{
			GameState.GameStateKind stateKind = GameHubBehaviour.Hub.State.Current.StateKind;
			switch (stateKind)
			{
			case GameState.GameStateKind.Stater:
			case GameState.GameStateKind.MainMenu:
			case GameState.GameStateKind.Splash:
				break;
			default:
				if (stateKind != GameState.GameStateKind.HORTA)
				{
					ClientCounselorController.Log.ErrorFormat("GameOnOnCounselorActiveChanged: unexpected StateKind. StateKind={0}", new object[]
					{
						GameHubBehaviour.Hub.State.Current.StateKind
					});
					return;
				}
				break;
			case GameState.GameStateKind.Pick:
			case GameState.GameStateKind.Game:
				GameHubBehaviour.Hub.Characters.Async().ClientSendCounselorActivation(GameHubBehaviour.Hub.Options.Game.CounselorActive);
				return;
			}
		}

		private void OnDisable()
		{
			GameHubBehaviour.Hub.State.ListenToStateChanged -= this.State_ListenToStateChanged;
			GameHubBehaviour.Hub.Options.Game.OnCounselorActiveChanged -= this.GameOnOnCounselorActiveChanged;
		}

		private void Update()
		{
			if (this.IsAudioValid || GameHubBehaviour.Hub.State.Current.StateKind != GameState.GameStateKind.Game || SpectatorController.IsSpectating)
			{
				return;
			}
			if (this._isPlayingAudio)
			{
				this._isPlayingAudio = false;
				this._lastPlayedAdviceIndex = this._currentAdviceConfigIndex;
				if (this.OnAudioPlayingChanged != null)
				{
					this.OnAudioPlayingChanged();
				}
			}
			if (!GameHubBehaviour.Hub.Options.Game.CounselorActive || GameHubBehaviour.Hub.Players.CurrentPlayerData.IsBotControlled)
			{
				return;
			}
			int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			for (int i = 0; i < this._adviceStatus.Length; i++)
			{
				if (this._adviceStatus[i].Active && this._adviceStatus[i].RemainingUses > 0)
				{
					CounselorConfig.AdvicesConfig adviceConfig = GameHubBehaviour.Hub.CounselorConfig.Advices[i];
					if (adviceConfig.AudioAsset != null && this.IsCooldownReady(i, playbackTime) && this.IsDependencyAble(i))
					{
						this._adviceStatus[i].RemainingUses = this._adviceStatus[i].RemainingUses - 1;
						this.PlayAdviceAudio(adviceConfig, i);
						break;
					}
				}
			}
		}

		private bool IsDependencyAble(int index)
		{
			return this._adviceStatus[index].dependencyIndex < 0 || this._adviceStatus[index].dependencyIndex == this._lastPlayedAdviceIndex;
		}

		private void PlayAdviceAudio(CounselorConfig.AdvicesConfig adviceConfig, int index)
		{
			GameHubBehaviour.Hub.AnnouncerAudio.StopAll();
			FMODVoiceOverAsset fmodvoiceOverAsset = adviceConfig.AudioAsset;
			if (adviceConfig.ControlAction > ControlAction.None && adviceConfig.AlternativeAudioAsset != null)
			{
				KeyboarLayout keyboarLayout = GameHubBehaviour.Hub.Options.Controls.GetCurrentKeyboardLayout();
				Control controlByAction = keyboarLayout.GetControlByAction(adviceConfig.ControlAction);
				string cinputText;
				if (controlByAction == null)
				{
					keyboarLayout = GameHubBehaviour.Hub.Options.Controls.GetDefaultKeyboardLayout();
					controlByAction = keyboarLayout.GetControlByAction(adviceConfig.ControlAction);
					cinputText = ControlOptions.GetCInputText(controlByAction.Action, ControlOptions.ControlActionInputType.Primary);
				}
				else
				{
					cinputText = ControlOptions.GetCInputText(controlByAction.Action, ControlOptions.ControlActionInputType.Primary);
				}
				if (adviceConfig.AlternativeKeyTrigger == cinputText)
				{
					fmodvoiceOverAsset = adviceConfig.AlternativeAudioAsset;
				}
			}
			this._currentPlayingAudio = FMODAudioManager.PlayAtVolume(fmodvoiceOverAsset, base.transform, 1f, false);
			this._isPlayingAudio = true;
			this.AddCooldown(index, fmodvoiceOverAsset.Cooldown);
			this._currentAdviceConfigIndex = index;
			if (this.OnAudioPlayingChanged != null)
			{
				this.OnAudioPlayingChanged();
			}
		}

		private bool IsCooldownReady(int index, int currentTime)
		{
			return this._adviceStatus[index].cooldown <= (float)currentTime;
		}

		private void AddCooldown(int index, float cooldown)
		{
			if (cooldown > 0f)
			{
				this._adviceStatus[index].cooldown = cooldown * 1000f + (float)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			}
		}

		private void State_ListenToStateChanged(GameState changedState)
		{
			if (changedState.StateKind == GameState.GameStateKind.Game)
			{
				return;
			}
			for (int i = 0; i < this._adviceStatus.Length; i++)
			{
				int maxUsesPerGame = GameHubBehaviour.Hub.CounselorConfig.Advices[i].MaxUsesPerGame;
				if (maxUsesPerGame == 0)
				{
					this._adviceStatus[i].RemainingUses = int.MaxValue;
				}
				else
				{
					this._adviceStatus[i].RemainingUses = maxUsesPerGame;
				}
				this._adviceStatus[i].Active = false;
				this._adviceStatus[i].cooldown = 0f;
			}
		}

		public void UpdateAdvice(int configIndex, bool isActive)
		{
			if (!GameHubBehaviour.Hub.Options.Game.CounselorActive || SpectatorController.IsSpectating)
			{
				return;
			}
			this._adviceStatus[configIndex].Active = isActive;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ClientCounselorController));

		private FMODAudioManager.FMODAudio _currentPlayingAudio = new FMODAudioManager.FMODAudio(-1);

		private ClientCounselorController.AdviceStatus[] _adviceStatus = new ClientCounselorController.AdviceStatus[0];

		private bool _isPlayingAudio;

		private int _lastPlayedAdviceIndex = -1;

		private int _currentAdviceConfigIndex;

		private struct AdviceStatus
		{
			public bool Active;

			public int RemainingUses;

			public CounselorConfig.AdvicesConfig config;

			public float cooldown;

			public int dependencyIndex;
		}
	}
}
