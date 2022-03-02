using System;
using System.Diagnostics;
using FMod;
using HeavyMetalMachines.Infra.Counselor;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Spectator;
using Hoplon.Input;
using Hoplon.Input.Business;
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
				if (stateKind != GameState.GameStateKind.HORTA && stateKind != GameState.GameStateKind.Welcome)
				{
					ClientCounselorController.Log.WarnFormat("GameOnOnCounselorActiveChanged: unexpected StateKind. StateKind={0}", new object[]
					{
						GameHubBehaviour.Hub.State.Current.StateKind
					});
					return;
				}
				break;
			case GameState.GameStateKind.Pick:
			case GameState.GameStateKind.Game:
				ClientCounselorController.Log.DebugFormat("GameOnOnCounselorActiveChanged: ClientSendCounselorActivation on StateKind={0}", new object[]
				{
					GameHubBehaviour.Hub.State.Current.StateKind
				});
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
			if (this.IsAudioValid || GameHubBehaviour.Hub.State.Current.StateKind != GameState.GameStateKind.Game || this._spectatorService.IsSpectating || GameHubBehaviour.Hub.Match.State == MatchData.MatchState.Nothing)
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
			ControllerInputActions inputAction = adviceConfig.InputAction;
			int actionId = inputAction;
			AudioEventAsset audioEventAsset;
			if (this._inputGetActiveDevicePoller.GetActiveDevice() == 3)
			{
				audioEventAsset = adviceConfig.JoystickAudioAsset;
				if (inputAction != -1 && adviceConfig.AlternativeAudioAsset != null)
				{
					if (inputAction == 4)
					{
						actionId = 2;
					}
					if (this._inputGetPlayerMapping.GetJoystickKeyFromAction(actionId) == adviceConfig.JoystickAlternativeKeyCode)
					{
						audioEventAsset = adviceConfig.JoystickAlternativeAudioAsset;
					}
				}
			}
			else
			{
				audioEventAsset = adviceConfig.AudioAsset;
				if (inputAction != -1 && adviceConfig.AlternativeAudioAsset != null)
				{
					KeyboardMouseCode primaryKeyFromAction = this._inputGetPlayerMapping.GetPrimaryKeyFromAction(actionId);
					if (primaryKeyFromAction == adviceConfig.AlternativeKeyCode)
					{
						audioEventAsset = adviceConfig.AlternativeAudioAsset;
					}
					else
					{
						KeyboardMouseCode secondaryKeyFromAction = this._inputGetPlayerMapping.GetSecondaryKeyFromAction(actionId);
						if (secondaryKeyFromAction == adviceConfig.AlternativeKeyCode)
						{
							audioEventAsset = adviceConfig.AlternativeAudioAsset;
						}
					}
				}
			}
			if (audioEventAsset != null)
			{
				GameHubBehaviour.Hub.AnnouncerAudio.StopAll();
				this._currentPlayingAudio = FMODAudioManager.PlayAtVolume(audioEventAsset, base.transform, 1f, false);
				this._isPlayingAudio = true;
				this.AddCooldown(index, audioEventAsset.Cooldown.Value);
				this._currentAdviceConfigIndex = index;
				if (this.OnAudioPlayingChanged != null)
				{
					this.OnAudioPlayingChanged();
				}
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
			ClientCounselorController.Log.DebugFormat("ClientCounselorController Initialization", new object[0]);
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
			if (!GameHubBehaviour.Hub.Options.Game.CounselorActive || this._spectatorService.IsSpectating)
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

		[InjectOnClient]
		private IInputGetPlayerMapping _inputGetPlayerMapping;

		[InjectOnClient]
		private IInputGetActiveDevicePoller _inputGetActiveDevicePoller;

		[InjectOnClient]
		private ISpectatorService _spectatorService;

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
