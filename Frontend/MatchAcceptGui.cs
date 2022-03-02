using System;
using System.Collections;
using System.Collections.Generic;
using FMod;
using HeavyMetalMachines.BI;
using Holoville.HOTween;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class MatchAcceptGui : GameHubBehaviour
	{
		public bool Visible
		{
			get
			{
				return this._visible;
			}
		}

		public void SetTitleText(string txt)
		{
			this.titleLabel.text = txt;
		}

		private IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		private IUiNavigationAxisSelector UiNavigationAxisSelector
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

		private void Start()
		{
			this.MainMenuGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>();
			GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnMatchAcceptedEvent += this.OnMatchAcceptedEvent;
			GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnMatchCanceledEvent += this.OnMatchCanceledEvent;
			this.HideAcceptanceWindow(false);
		}

		private void OnDestroy()
		{
			if (GameHubBehaviour.Hub.Swordfish == null)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Swordfish.Msg == null)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking == null)
			{
				return;
			}
			GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnMatchAcceptedEvent -= this.OnMatchAcceptedEvent;
			GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnMatchCanceledEvent -= this.OnMatchCanceledEvent;
		}

		private void OnMatchCanceledEvent(string[] clients)
		{
			this.AddClientsAnswer(clients, false);
		}

		private void OnMatchAcceptedEvent(string[] clients)
		{
			this.AddClientsAnswer(clients, true);
		}

		private void AddClientsAnswer(string[] clients, bool accepted)
		{
			if (!this.acceptPanel.gameObject.activeSelf)
			{
				return;
			}
			if (clients == null)
			{
				MatchAcceptGui.Log.WarnFormat("NULL clients when receiving answer from matchmaking. accepted: {0}", new object[]
				{
					accepted
				});
				return;
			}
			foreach (string text in clients)
			{
				if (!string.IsNullOrEmpty(text))
				{
					if (!this._clientsAnswerGuids.Contains(text))
					{
						this._clientsAnswerGuids.Add(text);
						for (int j = 0; j < this.playerSprites.Length; j++)
						{
							if (!this.playerSprites[j].gameObject.activeSelf)
							{
								this.playerSprites[j].gameObject.SetActive(true);
								this.playerSprites[j].sprite2D = ((!accepted) ? this.redSkull : this.greenSkull);
								FMODAudioManager.PlayOneShotAt((!accepted) ? this.refuseAudio : this.acceptAudio, Vector3.zero, 0);
								if (!accepted)
								{
									this.hintLabel.gameObject.SetActive(true);
								}
								break;
							}
						}
					}
				}
			}
			if (!accepted)
			{
				this.HideAcceptanceWindow(true);
			}
		}

		private void ResetPlayersAcceptance()
		{
			this._clientsAnswerGuids.Clear();
			for (int i = 0; i < this.playerSprites.Length; i++)
			{
				UI2DSprite ui2DSprite = this.playerSprites[i];
				ui2DSprite.gameObject.SetActive(false);
			}
		}

		public void ShowMatchConfirmation(int numBots, int matchMakingAcceptTimeout, Guid matchId)
		{
			this.TryToCloseShopWindow();
			if (!this._visible)
			{
				this.ResetPlayersAcceptance();
			}
			this._matchAccepted = false;
			this._matchId = matchId;
			this._totalMatchAcceptTimeout = matchMakingAcceptTimeout;
			this._visible = true;
			this._matchAcceptTimeoutTimer = Time.time + (float)this._totalMatchAcceptTimeout / 1000f;
			this.hintLabel.gameObject.SetActive(false);
			this.WaintingPlayersFeedback.SetActive(true);
			this._desactiveWaintingPlayersFeedback = false;
			this.acceptButton.gameObject.SetActive(true);
			this.refuseButton.gameObject.SetActive(true);
			this.acceptPanel.alpha = 1f;
			this.acceptPanel.gameObject.SetActive(true);
			this.playersGroup.SetActive(true);
			this.progressTimerGroup.SetActive(true);
			this.Animations.Play(this.ShowAnimationName);
			if (this._animating)
			{
				HOTween.Kill(this.acceptPanel);
				this._animating = false;
			}
			this.nextBotAcceptance = Time.time + this.TimeForBotAccept;
			this.remainingBots = numBots;
			this.UiNavigationGroupHolder.AddHighPriorityGroup();
			this._matchAcceptScreenBiLogger.LogOpen(matchId);
		}

		private void Update()
		{
			if (!this._visible || !this.acceptPanel.gameObject.activeSelf)
			{
				return;
			}
			float num = this._matchAcceptTimeoutTimer - Time.time;
			if (num < 0f)
			{
				num = 0f;
				if (!this._matchAccepted && !this.MainMenuGui.GameModesGui.IsPlayerBlocked())
				{
					this.MainMenuGui.GameModesGui.BlockPlayer();
					this._matchAcceptScreenBiLogger.LogTimeout(this._matchId);
				}
			}
			TimeSpan timeSpan = TimeSpan.FromSeconds((double)num);
			if (this.progressTimerGroup.activeSelf)
			{
				this.progressTimer.value = num / ((float)this._totalMatchAcceptTimeout / 1000f);
			}
			this.timerLabel.text = string.Format("{0}:{1}", timeSpan.Minutes.ToString("00"), timeSpan.Seconds.ToString("00"));
			if (this.remainingBots > 0)
			{
				if (Time.time > this.nextBotAcceptance)
				{
					for (int i = 0; i < this.playerSprites.Length; i++)
					{
						if (!this.playerSprites[i].gameObject.activeSelf)
						{
							this.playerSprites[i].gameObject.SetActive(true);
							this.playerSprites[i].sprite2D = this.greenSkull;
							FMODAudioManager.PlayOneShotAt(this.acceptAudio, Vector3.zero, 0);
							this.remainingBots--;
							this.nextBotAcceptance = Time.time + this.TimeForBotAccept;
							break;
						}
					}
				}
			}
			else if (this._desactiveWaintingPlayersFeedback)
			{
				this._desactiveWaintingPlayersFeedback = false;
				this.WaintingPlayersFeedback.SetActive(false);
			}
		}

		public void OnClickMatchAccept()
		{
			HOTween.Kill(this.acceptPanel);
			ScreenResolutionController.StopFlashWindow();
			ScreenResolutionController.EndTopmostWindow();
			this.acceptButton.gameObject.SetActive(false);
			this.refuseButton.gameObject.SetActive(false);
			this.progressTimerGroup.SetActive(false);
			this._matchAccepted = true;
			this.MainMenuGui.MatchStats.OnMatchAccepted(GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.MatchMadeQueue);
			this._matchAcceptScreenBiLogger.LogAccept(this._matchId);
		}

		private void TryToCloseShopWindow()
		{
			if (GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.gameObject.activeSelf)
			{
				GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.CloseWindow();
			}
		}

		public void OnClickMatchRefuse()
		{
			this._matchAcceptScreenBiLogger.LogReject(this._matchId);
			HOTween.Kill(this.acceptPanel);
			this.HideAcceptanceWindow(true);
			this.MainMenuGui.RejectMatch();
			this.MainMenuGui.GameModesGui.BlockPlayer();
		}

		public void HideAcceptanceWindow(bool animate = false)
		{
			this._visible = false;
			this._matchId = Guid.Empty;
			this.UiNavigationGroupHolder.RemoveHighPriorityGroup();
			this.UiNavigationAxisSelector.ClearSelection();
			ScreenResolutionController.StopFlashWindow();
			ScreenResolutionController.EndTopmostWindow();
			if (this._animating || !this.acceptPanel.gameObject.activeSelf)
			{
				return;
			}
			this.acceptButton.gameObject.SetActive(false);
			this.refuseButton.gameObject.SetActive(false);
			if (animate)
			{
				base.StartCoroutine(this.HideAcceptWindownAnimation());
				return;
			}
			this.playersGroup.SetActive(false);
			this.progressTimerGroup.SetActive(false);
			this.acceptPanel.gameObject.SetActive(false);
		}

		public void WaitingServerStart()
		{
			this._desactiveWaintingPlayersFeedback = true;
		}

		private IEnumerator HideAcceptWindownAnimation()
		{
			this.Animations.Play(this.HideAnimationName);
			while (this.Animations.IsPlaying(this.HideAnimationName))
			{
				yield return null;
			}
			this.playersGroup.SetActive(false);
			this.progressTimerGroup.SetActive(false);
			this.acceptPanel.gameObject.SetActive(false);
			yield break;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(MatchAcceptGui));

		private bool _matchAccepted;

		public UIPanel acceptPanel;

		public ButtonScriptReference acceptButton;

		public ButtonScriptReference refuseButton;

		public UILabel timerLabel;

		public UILabel titleLabel;

		public UI2DSprite[] playerSprites;

		public GameObject playersGroup;

		public GameObject progressTimerGroup;

		public GameObject WaintingPlayersFeedback;

		public UIProgressBar progressTimer;

		public UILabel hintLabel;

		public Sprite greenSkull;

		public Sprite redSkull;

		public Animation Animations;

		public string HideAnimationName;

		public string ShowAnimationName;

		public AudioEventAsset acceptAudio;

		public AudioEventAsset refuseAudio;

		private MainMenuGui MainMenuGui;

		public float TimeForBotAccept;

		private float nextBotAcceptance;

		private int remainingBots;

		private int _totalMatchAcceptTimeout;

		private bool _desactiveWaintingPlayersFeedback;

		private bool _visible;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private UiNavigationAxisSelector _uiNavigationAxisSelector;

		[Inject]
		private IClientMatchAcceptScreenBILogger _matchAcceptScreenBiLogger;

		private Guid _matchId;

		private List<string> _clientsAnswerGuids = new List<string>();

		private float _matchAcceptTimeoutTimer;

		private bool _animating;
	}
}
