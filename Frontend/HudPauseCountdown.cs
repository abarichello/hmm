using System;
using System.Collections;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using FMod;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudPauseCountdown : GameHubBehaviour
	{
		public static HudPauseCountdown Instance
		{
			get
			{
				return HudPauseCountdown.instance;
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
			if (HudPauseCountdown.instance == null)
			{
				HudPauseCountdown.instance = this;
			}
			else
			{
				Debug.LogError("[HudPauseCountdown] Multiple instances of HudInGamePause created. It should have only one.", this);
			}
			this.currentTimeSecond = 0;
			this.countdownToPauseTimerLabel.text = string.Empty;
		}

		private void OnDestroy()
		{
			this._gameGui = null;
			if (HudPauseCountdown.instance == this)
			{
				HudPauseCountdown.instance = null;
			}
		}

		private void Update()
		{
			if (this._countdownTimer <= 0f)
			{
				return;
			}
			this._countdownTimer = Mathf.Max(this._countdownTimer - Time.unscaledDeltaTime, 0f);
			this.SetCountdownTimer(this._countdownTimer);
		}

		public void OnFirstAnimationFinished()
		{
			this._pauseCountdownAnimationFinished = true;
		}

		public void ShowCountdownToPause(float countdownDuration, bool showAsBlue, bool playTopBarAnimation = false)
		{
			if (playTopBarAnimation)
			{
				this.hudTopBar.OpenTopBar();
			}
			this._countdownTimer = countdownDuration;
			this.currentTimeSecond = -1;
			this.countdownToPauseTimerLabel.text = string.Empty;
			HudInGamePause.Instance.ShowBorderOverlay(true);
			if (playTopBarAnimation)
			{
				this._pauseCountdownAnimationFinished = false;
				this.pauseCountdownContentAnimator.SetBool("active", true);
				this.glowAnimator.SetBool("active", true);
			}
			this.SetTeamVisualProperties(showAsBlue);
			HudInGamePause.Instance.SetBorderOverlayBehind(true);
		}

		public void HidePauseCountdown()
		{
			if (!SpectatorController.IsSpectating && GameHubBehaviour.Hub.Players.CurrentPlayerData.IsBotControlled)
			{
				this.hudTopBar.CloseTopBar();
			}
			this._countdownTimer = -1f;
			this.pauseCountdownContentAnimator.SetBool("active", false);
			this.glowAnimator.SetBool("active", false);
		}

		private void SetCountdownTimer(float countdownNumber)
		{
			int num = Mathf.CeilToInt(countdownNumber);
			if (num > 0 && num != this.currentTimeSecond)
			{
				this.currentTimeSecond = num;
				if (this._pauseCountdownAnimationFinished)
				{
					base.StartCoroutine(this.PlayNumberAnimation());
				}
			}
		}

		private IEnumerator PlayNumberAnimation()
		{
			this.numberAnimator.SetTrigger("ShowNumber");
			yield return base.StartCoroutine(UnityUtils.WaitForSecondsRealTime(0.1f));
			FMODAudioManager.PlayOneShotAt(this.timeTickAudioAsset, Vector3.zero, 0);
			this.countdownToPauseTimerLabel.text = this.currentTimeSecond.ToString();
			this.GameGui.HudChatController.OnPauseCountDown(this.currentTimeSecond);
			PauseController.Instance.InvokeInGameCountdownNotification(this.currentTimeSecond);
			yield break;
		}

		private void SetTeamVisualProperties(bool showAsBlue)
		{
			this.pauseIconSprite.sprite2D = ((!showAsBlue) ? this.redPauseIcon : this.bluePauseIcon);
			this.borderSprite.sprite2D = ((!showAsBlue) ? this.redBorder : this.blueBorder);
			this.glowSprite.sprite2D = ((!showAsBlue) ? this.redGlow : this.blueGlow);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HudPauseCountdown));

		[SerializeField]
		private Animator pauseCountdownContentAnimator;

		[SerializeField]
		private UILabel countdownToPauseTimerLabel;

		[SerializeField]
		private UILabel countdownToPauseTitleLabel;

		[SerializeField]
		private FMODAsset timeTickAudioAsset;

		[SerializeField]
		private string pauseLabelDraft;

		[SerializeField]
		private Animator glowAnimator;

		[SerializeField]
		private Animator numberAnimator;

		[SerializeField]
		private UI2DSprite pauseIconSprite;

		[SerializeField]
		private UI2DSprite borderSprite;

		[SerializeField]
		private UI2DSprite glowSprite;

		[SerializeField]
		private Sprite bluePauseIcon;

		[SerializeField]
		private Sprite redPauseIcon;

		[SerializeField]
		private Sprite blueBorder;

		[SerializeField]
		private Sprite redBorder;

		[SerializeField]
		private Sprite blueGlow;

		[SerializeField]
		private Sprite redGlow;

		[SerializeField]
		private HudTopBar hudTopBar;

		private static HudPauseCountdown instance;

		private int currentTimeSecond;

		private float _countdownTimer;

		private bool _pauseCountdownAnimationFinished;

		private GameGui _gameGui;
	}
}
