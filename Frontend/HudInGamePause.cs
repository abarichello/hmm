using System;
using System.Collections;
using FMod;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.Pause;
using HeavyMetalMachines.PostProcessing;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.VFX;
using Hoplon.Input;
using Pocketverse;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudInGamePause : HudWindow, IPausePresenter
	{
		public bool Visible
		{
			get
			{
				return this.IsVisible;
			}
		}

		public IObservable<bool> VisibilityChanged()
		{
			return this._visibilityObservation;
		}

		public static HudInGamePause Instance
		{
			get
			{
				return HudInGamePause.instance;
			}
		}

		private GameGui GameGui
		{
			get
			{
				GameGui result;
				if ((result = this._gameGui) == null)
				{
					result = (this._gameGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<GameGui>());
				}
				return result;
			}
		}

		private void Awake()
		{
			if (HudInGamePause.instance == null)
			{
				HudInGamePause.instance = this;
			}
			else
			{
				Debug.LogError("[HudInGamePause] Multiple instances of HudInGamePause created. It should have only one.", this);
			}
			this.currentUnpauseSecond = -1;
			this.ChangeWindowVisibility(false);
			this.SetUnpauseCountdownTimer(0f);
			this._visibilityObservation = new Subject<bool>();
			PauseController.OnNotification += this.OnNotification;
		}

		private void Start()
		{
			this.UpdateKeyInfo();
			this.descriptionLabel.text = Language.Get("PAUSE_GAME_UNPAUSE_HINT", TranslationContext.GUI);
			this._inputActiveDeviceChangeNotifierDisposable = ObservableExtensions.Subscribe<InputDevice>(Observable.Do<InputDevice>(this._inputActiveDeviceChangeNotifier.ObserveActiveDeviceChange(), delegate(InputDevice activeDevice)
			{
				this.UpdateKeyInfo();
			}));
		}

		private void UpdateKeyInfo()
		{
			bool flag = this._inputGetActiveDevicePoller.GetActiveDevice() == 3;
			ControllerInputActions controllerInputActions = (!flag) ? 20 : 50;
			ISprite sprite;
			string text;
			this._inputTranslation.TryToGetInputActionActiveDeviceAssetOrFallbackToTranslation(controllerInputActions, ref sprite, ref text);
			if (flag)
			{
				this.KeyGameObject.SetActive(false);
				this.descriptionKeySprite.transform.parent.gameObject.SetActive(true);
				this.descriptionKeySprite.sprite2D = (sprite as UnitySprite).GetSprite();
				return;
			}
			this.descriptionKeySprite.transform.parent.gameObject.SetActive(false);
			this.KeyGameObject.SetActive(true);
			this.KeyLabel.text = text;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			PauseController.OnNotification -= this.OnNotification;
			this._visibilityObservation.Dispose();
			if (HudInGamePause.instance == this)
			{
				HudInGamePause.instance = null;
			}
			if (this._inputActiveDeviceChangeNotifierDisposable != null)
			{
				this._inputActiveDeviceChangeNotifierDisposable.Dispose();
				this._inputActiveDeviceChangeNotifierDisposable = null;
			}
		}

		private void Update()
		{
			if (this.IsVisible)
			{
				if (this.unpauseCountdownTimer > 0f)
				{
					this.unpauseCountdownTimer -= Time.unscaledDeltaTime;
					if (this.unpauseCountdownTimer < 0f)
					{
						this.unpauseCountdownTimer = 0f;
					}
					this.SetUnpauseCountdownTimer(this.unpauseCountdownTimer);
				}
				if (this._currentSettings.ShouldUseTimePool && this._currentSettings.ShouldCheckActivationLimit && PauseController.Instance.CurrentState == PauseController.PauseState.Paused)
				{
					PlayerData lastPlayerWhoToggled = PauseController.Instance.GetLastPlayerWhoToggled();
					if (!lastPlayerWhoToggled.IsNarrator)
					{
						PauseController.TeamPauseData pauseDataOnClient = PauseController.Instance.GetPauseDataOnClient(lastPlayerWhoToggled.Team);
						int num = (pauseDataOnClient.TimeoutMillis - GameHubBehaviour.Hub.GameTime.GetSynchTime()) / 1000;
						int num2 = Mathf.FloorToInt((float)num);
						this.SetTeamActivation(pauseDataOnClient.ActivationRemaining);
						this.SetTeamCountdownTimer(num2);
						if (num2 != this.previousTeamCountdownTime)
						{
							this.RecolorSideFeeback(num2 > 0 && pauseDataOnClient.ActivationRemaining > 0);
							this.previousTeamCountdownTime = num2;
						}
					}
				}
			}
			if (this.blockMessageTimer > 0f)
			{
				this.blockMessageTimer -= Time.unscaledDeltaTime;
				if (this.blockMessageTimer < 0f)
				{
					this.HideBlockInputMessage();
				}
			}
		}

		public override void ChangeWindowVisibility(bool visible)
		{
			bool isVisible = this.IsVisible;
			base.ChangeWindowVisibility(visible);
			if (isVisible != this.IsVisible)
			{
				this._visibilityObservation.OnNext(this.IsVisible);
			}
		}

		private InGamePauseTeamConfiguration GetInGamePauseTeamConfiguration(PlayerData playerWhoToggled)
		{
			if (playerWhoToggled.Team == GameHubBehaviour.Hub.Players.CurrentPlayerTeam)
			{
				return this.allyTeamConfiguration;
			}
			return this.enemyTeamConfiguration;
		}

		public void OnPauseTitleAnimationEnd()
		{
			if (!this._pauseTitleOutAnimationStarted)
			{
				return;
			}
			this._pauseTitleOutAnimationEnded = true;
			HudInGamePause.Log.DebugFormat("OnPauseTitleAnimationEnd", new object[0]);
			if (!PauseController.Instance.IsGamePaused)
			{
				HudInGamePause.Log.DebugFormat("game is not paused, will not start countdown animation. curent state {0}", new object[]
				{
					PauseController.Instance.IsGamePaused
				});
				return;
			}
			this.unpauseCountdownTextAnimator.SetBool("active", true);
			this.unpauseCounterAnimator.gameObject.SetActive(true);
		}

		public void ShowPauseState(bool show, float delayTime = 0f)
		{
			if (this.showPauseCoroutine != null)
			{
				HudInGamePause.Log.DebugFormat("Stop old coroutine", new object[0]);
				base.StopCoroutine(this.showPauseCoroutine);
			}
			if (show)
			{
				this.InitializeHudElements();
			}
			this.showPauseCoroutine = base.StartCoroutine(this.ShowPauseStateCoroutine(show, delayTime));
		}

		private void InitializeHudElements()
		{
			this._currentSettings = PauseController.Instance.PauseSettingsData;
			PlayerData lastPlayerWhoToggled = PauseController.Instance.GetLastPlayerWhoToggled();
			InGamePauseTeamConfiguration inGamePauseTeamConfiguration = this.GetInGamePauseTeamConfiguration(lastPlayerWhoToggled);
			HudInGamePause.Log.DebugFormat("playerWhoToggled {0} currentTeamConfiguration {1} _currentSettings {2}", new object[]
			{
				lastPlayerWhoToggled,
				inGamePauseTeamConfiguration.TitleLabel,
				this._currentSettings.InitialTimePoolForTeam
			});
			if (this._currentSettings.ShouldCheckActivationLimit)
			{
				this.sidePanelGameObject.SetActive(true);
				PauseController.TeamPauseData pauseDataOnClient = PauseController.Instance.GetPauseDataOnClient(lastPlayerWhoToggled.Team);
				this.SetTeamActivation(pauseDataOnClient.ActivationRemaining);
				int num = (pauseDataOnClient.TimeoutMillis - GameHubBehaviour.Hub.GameTime.GetSynchTime()) / 1000;
				this.SetTeamCountdownTimer(num);
				this.previousTeamCountdownTime = num;
				this.RecolorSideFeeback(num > 0 && pauseDataOnClient.ActivationRemaining > 0);
			}
			else
			{
				this.sidePanelGameObject.SetActive(false);
			}
			this.TeamBackgroundSprite.color = inGamePauseTeamConfiguration.TeamBackgroundColor;
			this.sideFeedbackBackgroundSprite.SpriteName = inGamePauseTeamConfiguration.SideFeedbackBackgroundSpriteName;
			this.titleLabel.text = Language.Get(inGamePauseTeamConfiguration.TitleLabel, TranslationContext.Hud);
		}

		private void RecolorSideFeeback(bool isOnCooldown)
		{
			if (isOnCooldown)
			{
				this.teamCountdownTimerLabel.color = this.LabelCountColorOnCountdown;
				this.teamActivationLabel.color = this.LabelCountColorOnCountdown;
				this.ChangeSideFeedbackSpritesColor(this.IconColorOnCountdown);
				this.ChangeSideFeedbackLabelColor(this.LabelColorOnCountdown);
			}
			else
			{
				this.teamActivationLabel.color = this.LabelCountColorOnTimeout;
				this.teamCountdownTimerLabel.color = this.LabelCountColorOnTimeout;
				this.ChangeSideFeedbackSpritesColor(this.IconColorOnTimeout);
				this.ChangeSideFeedbackLabelColor(this.LabelColorOnTimeout);
			}
		}

		private IEnumerator ShowPauseStateCoroutine(bool show, float delayToStart)
		{
			HudInGamePause.Log.DebugFormat("ShowPauseStateCoroutine show: {0} - visible: {1} - delayToStart: {2}", new object[]
			{
				show,
				this.IsVisible,
				delayToStart
			});
			yield return base.StartCoroutine(UnityUtils.WaitForSecondsRealTime(delayToStart));
			HudInGamePause.Log.DebugFormat("ShowPauseStateCoroutine afterdelay show: {0} - visible: {1} - delayToStart: {2}", new object[]
			{
				show,
				this.IsVisible,
				delayToStart
			});
			if (show)
			{
				if (!this.IsVisible)
				{
					this.unpauseCountdownTimer = 0f;
					this.SetUnpauseCountdownTimer(0f);
					this._pauseTitleOutAnimationStarted = false;
					this._pauseTitleOutAnimationEnded = false;
					this.pausedTitleAnimator.SetBool("active", true);
					this.unpauseCountdownTextAnimator.SetBool("active", false);
					this.unpauseCounterAnimator.gameObject.SetActive(false);
					base.ShowFromUI();
					GameHubBehaviour.Hub.AnnouncerAudio.PlayPauseAudio();
					this.SetBorderOverlayBehind(false);
					PostProcessingState postProcessingState = this._postProcessing.Request("Pause", () => PauseController.Instance.IsGamePaused, false);
					if (postProcessingState != null)
					{
						postProcessingState.Enabled = true;
						postProcessingState.CRTMonitor.Enabled = true;
						postProcessingState.CRTMonitor.Parameters.LineStrength = 0.5f;
						postProcessingState.Saturation.Enabled = true;
						postProcessingState.Saturation.Parameters.Value = 0.5f;
					}
				}
			}
			else if (this.IsVisible)
			{
				this._pauseTitleOutAnimationStarted = true;
				this._pauseTitleOutAnimationEnded = false;
				this.pausedTitleAnimator.SetBool("active", false);
				this.unpauseCountdownTextAnimator.SetBool("active", false);
				this.unpauseCounterAnimator.gameObject.SetActive(false);
				this.ShowBorderOverlay(false);
				base.HideFromUI();
				this.HideBlockInputMessage();
				GameHubBehaviour.Hub.AnnouncerAudio.PlayUnpauseAudio();
			}
			HudInGamePause.Log.DebugFormat("ShowPauseStateCoroutine done", new object[0]);
			yield break;
		}

		public void ShowPauseScreenBorder(bool show)
		{
			this.borderAnimator.SetBool("ShowBorder", show);
		}

		public void ClearTeamLabels()
		{
			this.teamActivationLabel.text = string.Empty;
			this.teamCountdownTimerLabel.text = string.Empty;
		}

		public void StartUnpauseCountdownTimer(float countdownDuration)
		{
			this.unpauseCountdownTimer = countdownDuration;
			this.countdownTimerLabel.text = string.Empty;
			this._pauseTitleOutAnimationStarted = true;
			this._pauseTitleOutAnimationEnded = false;
			this.pausedTitleAnimator.SetBool("active", false);
		}

		private void SetUnpauseCountdownTimer(float newCountdownTime)
		{
			int num = Mathf.CeilToInt(newCountdownTime);
			if (num > 0 && this.currentUnpauseSecond != num)
			{
				this.currentUnpauseSecond = num;
				if (this._pauseTitleOutAnimationEnded)
				{
					base.StartCoroutine(this.PlayNumberAnimation());
				}
				else
				{
					HudInGamePause.Log.DebugFormat("Still wait animation to activate time. Timer value:{0}", new object[]
					{
						this.currentUnpauseSecond
					});
				}
			}
		}

		private void ChangeSideFeedbackSpritesColor(Color color)
		{
			for (int i = 0; i < this.SideFeedbackChangeColorSprites.Length; i++)
			{
				this.SideFeedbackChangeColorSprites[i].color = color;
			}
		}

		private void ChangeSideFeedbackLabelColor(Color color)
		{
			for (int i = 0; i < this.SideFeedbackChangeColorLabels.Length; i++)
			{
				this.SideFeedbackChangeColorLabels[i].color = color;
			}
		}

		private void SetTeamActivation(int activation)
		{
			if (activation > 0)
			{
				this.teamActivationLabel.text = activation.ToString();
			}
			else
			{
				this.teamActivationLabel.text = "0";
			}
		}

		private void SetTeamCountdownTimer(int secondToBeShown)
		{
			if (secondToBeShown > 0)
			{
				this.teamCountdownTimerLabel.text = string.Format("{0:0}:{1:0#}", Mathf.Floor((float)(secondToBeShown / 60)), Mathf.Floor((float)(secondToBeShown % 60)));
			}
			else
			{
				this.teamCountdownTimerLabel.text = "0:00";
			}
		}

		private void RecolorCountdownTimer(int secondToBeShown)
		{
			if (secondToBeShown > 0)
			{
				this.teamCountdownTimerLabel.color = this.LabelCountColorOnCountdown;
				this.ChangeSideFeedbackSpritesColor(this.IconColorOnCountdown);
				this.ChangeSideFeedbackLabelColor(this.LabelColorOnCountdown);
			}
			else
			{
				this.teamActivationLabel.color = this.LabelCountColorOnTimeout;
				this.teamCountdownTimerLabel.color = this.LabelCountColorOnTimeout;
				this.ChangeSideFeedbackSpritesColor(this.IconColorOnTimeout);
				this.ChangeSideFeedbackLabelColor(this.LabelColorOnTimeout);
			}
		}

		private IEnumerator PlayNumberAnimation()
		{
			this.unpauseCounterAnimator.SetTrigger("ShowNumber");
			yield return base.StartCoroutine(UnityUtils.WaitForSecondsRealTime(0.1f));
			this.countdownTimerLabel.text = this.currentUnpauseSecond.ToString();
			this.GameGui.HudChatController.OnPauseCountDown(this.currentUnpauseSecond);
			PauseController.Instance.InvokeInGameCountdownNotification(this.currentUnpauseSecond);
			yield break;
		}

		public void ShowBlockInputMessage(float duration)
		{
			if (!this.blockMessageOn)
			{
				this.blockInputMessageAnimator.SetTrigger("PlayForward");
				this.blockMessageOn = true;
			}
			FMODAudioManager.PlayOneShotAt(this.blockActionAudioAsset, Vector3.zero, 0);
			this.blockMessageTimer = duration;
		}

		public void HideBlockInputMessage()
		{
			if (this.blockMessageOn)
			{
				Debug.Log("Hide Block TRIGGER Msg at " + Time.time);
				this.blockInputMessageAnimator.SetTrigger("PlayReverse");
				this.blockMessageOn = false;
			}
		}

		private void OnNotification(PauseController.PauseNotification notification)
		{
			if (!GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (this.IsVisible && notification.kind == PauseController.PauseNotificationKind.InputBlocked)
			{
				this.ShowBlockInputMessage(this.blockInputMessageDuration);
			}
		}

		public void ShowBorderOverlay(bool show)
		{
			this.borderAnimator.SetBool("active", show);
		}

		public void SetBorderOverlayBehind(bool setBehind)
		{
			this.borderPanel.depth = ((!setBehind) ? this.borderDepthOnPause : this.borderDepthOnCountdown);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HudInGamePause));

		[Header("In Game Pause")]
		[SerializeField]
		private UILabel countdownTimerLabel;

		[SerializeField]
		private UILabel titleLabel;

		[SerializeField]
		private UILabel descriptionLabel;

		[SerializeField]
		private UILabel KeyLabel;

		[SerializeField]
		private GameObject KeyGameObject;

		[SerializeField]
		private UI2DSprite descriptionKeySprite;

		[SerializeField]
		private UILabel teamCountdownTimerLabel;

		[SerializeField]
		private UILabel teamActivationLabel;

		[SerializeField]
		private UI2DSprite[] SideFeedbackChangeColorSprites;

		[SerializeField]
		private UILabel[] SideFeedbackChangeColorLabels;

		[SerializeField]
		private Color IconColorOnCountdown;

		[SerializeField]
		private Color IconColorOnTimeout;

		[SerializeField]
		private Color LabelColorOnCountdown;

		[SerializeField]
		private Color LabelColorOnTimeout;

		[SerializeField]
		private Color LabelCountColorOnCountdown;

		[SerializeField]
		private Color LabelCountColorOnTimeout;

		[SerializeField]
		private GameObject sidePanelGameObject;

		[SerializeField]
		private HMMUI2DDynamicSprite sideFeedbackBackgroundSprite;

		[SerializeField]
		private UI2DSprite TeamBackgroundSprite;

		[SerializeField]
		private float blockInputMessageDuration = 2.5f;

		[SerializeField]
		private Animator blockInputMessageAnimator;

		[SerializeField]
		private AudioEventAsset blockActionAudioAsset;

		[SerializeField]
		private Animator pausedTitleAnimator;

		[SerializeField]
		private Animator unpauseCountdownTextAnimator;

		[SerializeField]
		private Animator unpauseCounterAnimator;

		[SerializeField]
		private Animator borderAnimator;

		[SerializeField]
		private UIPanel borderPanel;

		[SerializeField]
		private int borderDepthOnCountdown = 2010;

		[SerializeField]
		private int borderDepthOnPause = 2013;

		[SerializeField]
		private InGamePauseTeamConfiguration allyTeamConfiguration;

		[SerializeField]
		private InGamePauseTeamConfiguration enemyTeamConfiguration;

		private static HudInGamePause instance;

		private float blockMessageTimer;

		private bool blockMessageOn;

		private float unpauseCountdownTimer;

		private int currentUnpauseSecond;

		private Coroutine showPauseCoroutine;

		private int previousTeamCountdownTime = -1;

		[InjectOnClient]
		private IInputTranslation _inputTranslation;

		[InjectOnClient]
		private IInputActiveDeviceChangeNotifier _inputActiveDeviceChangeNotifier;

		[InjectOnClient]
		private IInputGetActiveDevicePoller _inputGetActiveDevicePoller;

		[InjectOnClient]
		private IGamePostProcessing _postProcessing;

		private Subject<bool> _visibilityObservation;

		private IDisposable _inputActiveDeviceChangeNotifierDisposable;

		private GameGui _gameGui;

		private PauseSettings.PauseData _currentSettings;

		private bool _pauseTitleOutAnimationStarted;

		private bool _pauseTitleOutAnimationEnded;
	}
}
