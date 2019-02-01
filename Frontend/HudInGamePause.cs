using System;
using System.Collections;
using FMod;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.PostProcessing;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudInGamePause : HudWindow
	{
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
			PauseController.OnNotification += this.OnNotification;
		}

		private void Start()
		{
			string textlocalized = ControlOptions.GetTextlocalized(ControlAction.Pause, ControlOptions.ControlActionInputType.Primary);
			string text = Language.Get("PAUSE_GAME_UNPAUSE_HINT", TranslationSheets.GUI);
			text = string.Format(text, textlocalized);
			this.descriptionLabel.text = text;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			PauseController.OnNotification -= this.OnNotification;
			if (HudInGamePause.instance == this)
			{
				HudInGamePause.instance = null;
			}
		}

		private void Update()
		{
			if (this.IsVisible && this.unpauseCountdownTimer > 0f)
			{
				this.unpauseCountdownTimer -= Time.unscaledDeltaTime;
				if (this.unpauseCountdownTimer < 0f)
				{
					this.unpauseCountdownTimer = 0f;
				}
				this.SetUnpauseCountdownTimer(this.unpauseCountdownTimer);
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

		public void OnPauseTitleAnimationEnd()
		{
			if (!this._pauseTitleOutAnimationStarted)
			{
				return;
			}
			this._pauseTitleOutAnimationEnded = true;
			if (!PauseController.Instance.IsGamePaused)
			{
				return;
			}
			this.unpauseCountdownTextAnimator.SetBool("active", true);
			this.unpauseCounterAnimator.gameObject.SetActive(true);
		}

		public void ShowPauseState(bool show, float delayTime = 0f)
		{
			if (this.showPauseCoroutine != null)
			{
				base.StopCoroutine(this.showPauseCoroutine);
			}
			this.showPauseCoroutine = base.StartCoroutine(this.ShowPauseStateCoroutine(show, delayTime));
		}

		private IEnumerator ShowPauseStateCoroutine(bool show, float delayToStart)
		{
			yield return base.StartCoroutine(UnityUtils.WaitForSecondsRealTime(delayToStart));
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
					PostProcessingState postProcessingState = CarCamera.Singleton.postProcessing.Request("Pause", () => PauseController.Instance.IsGamePaused, false);
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
				base.HideFromUI();
				this.HideBlockInputMessage();
				this._pauseTitleOutAnimationStarted = true;
				this._pauseTitleOutAnimationEnded = false;
				this.pausedTitleAnimator.SetBool("active", false);
				this.unpauseCountdownTextAnimator.SetBool("active", false);
				this.unpauseCounterAnimator.gameObject.SetActive(false);
				this.ShowBorderOverlay(false);
				GameHubBehaviour.Hub.AnnouncerAudio.PlayUnpauseAudio();
			}
			yield break;
		}

		public void ShowPauseScreenBorder(bool show)
		{
			this.borderAnimator.SetBool("ShowBorder", show);
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
		private UILabel descriptionLabel;

		[SerializeField]
		private float blockInputMessageDuration = 2.5f;

		[SerializeField]
		private Animator blockInputMessageAnimator;

		[SerializeField]
		private FMODAsset blockActionAudioAsset;

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

		private static HudInGamePause instance;

		private float blockMessageTimer;

		private bool blockMessageOn;

		private float unpauseCountdownTimer;

		private int currentUnpauseSecond;

		private Coroutine showPauseCoroutine;

		private GameGui _gameGui;

		private bool _pauseTitleOutAnimationStarted;

		private bool _pauseTitleOutAnimationEnded;
	}
}
