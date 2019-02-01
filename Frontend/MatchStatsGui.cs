using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using HeavyMetalMachines.Audio;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX.PlotKids;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class MatchStatsGui : GameHubBehaviour
	{
		private void Awake()
		{
			this.NetClient = (NetworkClient)GameHubBehaviour.Hub.Net;
			this._cachedPlayBtnLocalized = Language.Get("SELECTION_MODE_START", TranslationSheets.MainMenuGui);
		}

		private void Start()
		{
			ManagerController.Get<GroupManager>().EvtInviteHandled += this.EvtInviteHandled;
			this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.None;
			this.ChangeState(MatchStatsGui.MatchmakingInterfaceState.None);
		}

		private void EvtInviteHandled(bool inviteAccepted)
		{
			if (!inviteAccepted)
			{
				return;
			}
			this.DisconnectFromMatch();
		}

		private void OnEnable()
		{
			GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnTimePredict += this.TimeToPlayPredictUpdated;
		}

		private void OnDestroy()
		{
			if (!SingletonMonoBehaviour<SocialController>.DoesInstanceExist())
			{
				return;
			}
			ManagerController.Get<GroupManager>().EvtInviteHandled -= this.EvtInviteHandled;
		}

		private void OnDisable()
		{
			try
			{
				GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnTimePredict -= this.TimeToPlayPredictUpdated;
			}
			catch (NullReferenceException arg)
			{
				if (HMMHub.IsEditorLeavingPlayMode())
				{
					MatchStatsGui.Log.Warn(string.Format("Safe to ignore exception, since we are in editor: {0}", arg));
				}
				else
				{
					MatchStatsGui.Log.Warn(string.Format("Ignoring NPE, since game is being closed {0}", arg));
				}
			}
		}

		private void TimeToPlayPredictUpdated(double obj)
		{
			string arg = Language.Get("QUEUE_AVG_TIME", TranslationSheets.MainMenuGui);
			if (obj < 0.0 || obj > 10.0)
			{
				this.TopAveargeTimeLabel.text = string.Format("{0}{1}", "+10", arg);
			}
			else
			{
				int num = (obj >= 1.0) ? ((int)Math.Ceiling(obj)) : 1;
				this.TopAveargeTimeLabel.text = string.Format("{0} {1}", num, arg);
			}
		}

		private void Update()
		{
			if (GameHubBehaviour.Hub.Swordfish.Msg == null || GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking == null)
			{
				return;
			}
			this.MatchmakingUpdate();
		}

		public void UpdateMatchmakingNow()
		{
			this.MatchmakingUpdate();
		}

		private void MatchmakingUpdate()
		{
			if (GameHubBehaviour.Hub.State.Current.StateKind != GameState.GameStateKind.MainMenu)
			{
				return;
			}
			this.CheckAndChangeState();
			float num = -1f;
			if (GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.State == SwordfishMatchmaking.MatchmakingState.None)
			{
				num = Time.time - this._matchmakingStartTime;
			}
			if (Mathf.Approximately(num, -1f))
			{
				return;
			}
			TimeSpan timeSpan = new TimeSpan(0, 0, (int)Math.Abs(num));
			int num2 = timeSpan.Minutes * 60 + timeSpan.Seconds;
			if (num2 == this._lastTextUpdateTimeTotalSeconds)
			{
				return;
			}
			this._lastTextUpdateTimeTotalSeconds = num2;
			string text = string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
			this.UpdateRealTimeTxt(text);
			if (this.TopPlayTimerLabel.gameObject.activeSelf)
			{
				this.TopPlayTimerLabel.text = text;
			}
		}

		[Obsolete]
		public void OnReconnectClick()
		{
			MatchStatsGui.Log.Warn("Called  obsolete OnReconnectClick!");
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.Get("RECONNECT_QUESTION", TranslationSheets.MainMenuGui),
				ConfirmButtonText = Language.Get("Accept", "GUI"),
				OnConfirm = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
					this.BackToGame();
				},
				RefuseButtonText = Language.Get("Refuse", "GUI"),
				OnRefuse = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
					this.DisconnectFromMatch();
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		public void BackToGame()
		{
			this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.WaitingServer;
			GameHubBehaviour.Hub.GuiScripts.Loading.ShowDefaultLoading(false);
			GameHubBehaviour.Hub.User.ReloadPlayer(new Action(this.GoBackToGame), new Action(this.GoBackToGame));
		}

		private void GoBackToGame()
		{
			if (GameHubBehaviour.Hub.User.Bag.CurrentPort <= 0)
			{
				MatchStatsGui.Log.ErrorFormat("Match ended while player was on main menu, no longer valid", new object[0]);
				GameHubBehaviour.Hub.GuiScripts.Loading.HideLoading();
				this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.None;
				Guid confirmWindowGuid = Guid.NewGuid();
				ConfirmWindowProperties properties = new ConfirmWindowProperties
				{
					Guid = confirmWindowGuid,
					QuestionText = Language.Get("MatchFailedToConnectMatchEnded", TranslationSheets.MainMenuGui),
					OkButtonText = Language.Get("Ok", TranslationSheets.GUI),
					OnOk = delegate()
					{
						GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
					}
				};
				GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
				return;
			}
			this._lastConnectionState = SessionState.Closed;
			GameHubBehaviour.Hub.Server.ServerIp = GameHubBehaviour.Hub.User.Bag.CurrentServerIp;
			GameHubBehaviour.Hub.Server.ServerPort = GameHubBehaviour.Hub.User.Bag.CurrentPort;
			GameHubBehaviour.Hub.Swordfish.Msg.ClientMatchId = new Guid(GameHubBehaviour.Hub.User.Bag.CurrentMatchId);
			GameHubBehaviour.Hub.User.ConnectToServer(true, delegate
			{
				GameHubBehaviour.Hub.State.GotoState(MatchStatsGui.MainMenuGui.Main, false);
			}, null);
		}

		private void CheckAndChangeState()
		{
			SwordfishMatchmaking matchmaking = GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking;
			switch (this.MMInterfaceState)
			{
			case MatchStatsGui.MatchmakingInterfaceState.None:
				this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.Play;
				break;
			case MatchStatsGui.MatchmakingInterfaceState.WaitingServer:
				if (this._lastConnectionState != this.NetClient.State)
				{
					string text = string.Empty;
					SessionState state = this.NetClient.State;
					if (state != SessionState.Opening)
					{
						if (state != SessionState.Established)
						{
							if (state == SessionState.Closed)
							{
								text = Language.Get("MatchClosed", "MainMenuGui");
								this.MMInterfaceState = ((!matchmaking.Connected) ? MatchStatsGui.MatchmakingInterfaceState.Play : MatchStatsGui.MatchmakingInterfaceState.OnTheLine);
								if (GameHubBehaviour.Hub.User.IsReconnecting)
								{
									GameHubBehaviour.Hub.User.IsReconnecting = false;
									MatchStatsGui.MainMenuGui.ClearCurrentServer();
								}
							}
						}
						else
						{
							text = Language.Get("MatchEstablished", "MainMenuGui");
						}
					}
					else
					{
						text = Language.Get("MatchOpening", "MainMenuGui");
					}
					if (!string.IsNullOrEmpty(text))
					{
						this.UpdateStatusTxt(text);
						MatchStatsGui.MainMenuGui.MatchAccept.SetTitleText(text);
					}
					this._lastConnectionState = this.NetClient.State;
				}
				if (matchmaking.State == SwordfishMatchmaking.MatchmakingState.None && !matchmaking.Undefined)
				{
					this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.Play;
				}
				else if (matchmaking.State != SwordfishMatchmaking.MatchmakingState.Started && !GameHubBehaviour.Hub.User.IsReconnecting)
				{
					if (!matchmaking.Undefined)
					{
						this.MMInterfaceState = ((!matchmaking.Connected) ? MatchStatsGui.MatchmakingInterfaceState.Play : MatchStatsGui.MatchmakingInterfaceState.OnTheLine);
						if (matchmaking.LastFailed)
						{
							Guid confirmWindowGuid = Guid.NewGuid();
							ConfirmWindowProperties properties = new ConfirmWindowProperties
							{
								Guid = confirmWindowGuid,
								QuestionText = Language.Get("MatchFailedToConnect", "MainMenuGui"),
								OkButtonText = Language.Get("Ok", "GUI"),
								OnOk = delegate()
								{
									GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
								}
							};
							GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
							GameHubBehaviour.Hub.User.ReloadPlayer(delegate
							{
								this.enabled = false;
								this.enabled = true;
							}, delegate
							{
								this.enabled = false;
								this.enabled = true;
							});
						}
					}
				}
				break;
			case MatchStatsGui.MatchmakingInterfaceState.Play:
				if (matchmaking.Connected)
				{
					this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.OnTheLine;
					this._matchmakingStartTime = Time.time;
				}
				break;
			case MatchStatsGui.MatchmakingInterfaceState.OnTheLine:
				MatchStatsGui.MainMenuGui.MatchAccept.HideAcceptanceWindow(false);
				if (!matchmaking.Connected)
				{
					this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.Play;
				}
				else if (matchmaking.State == SwordfishMatchmaking.MatchmakingState.Made)
				{
					this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.MatchMadeAsk;
				}
				break;
			case MatchStatsGui.MatchmakingInterfaceState.MatchMadeAsk:
				if (!matchmaking.Connected)
				{
					this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.Play;
					MatchStatsGui.MainMenuGui.MatchMakingCanceled();
				}
				else if (matchmaking.State == SwordfishMatchmaking.MatchmakingState.None)
				{
					this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.OnTheLine;
				}
				break;
			case MatchStatsGui.MatchmakingInterfaceState.MatchMadeWaiting:
				if (matchmaking.State == SwordfishMatchmaking.MatchmakingState.Started)
				{
					GameHubBehaviour.Hub.Swordfish.Msg.ConnectToMatch(MatchStatsGui.MainMenuGui.Main, new Action(this.FailedToConnect));
					this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.WaitingServer;
				}
				else if (!matchmaking.Connected && !matchmaking.WaitingForMatchResult)
				{
					this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.Play;
				}
				else if (matchmaking.State == SwordfishMatchmaking.MatchmakingState.None)
				{
					this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.OnTheLine;
				}
				break;
			}
		}

		private void FailedToConnect()
		{
			this.AnimateHideMatchSearchingPanel();
			this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.None;
			MatchStatsGui.MainMenuGui.MatchAccept.HideAcceptanceWindow(false);
			MatchStatsGui.MainMenuGui.UpdateMainMenuButtons();
		}

		public void DisconnectFromMatch()
		{
			MatchStatsGui.MainMenuGui.ClearCurrentServer();
			this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.Play;
		}

		private static MainMenuGui MainMenuGui
		{
			get
			{
				return GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>();
			}
		}

		public void AnimateShowMatchSearchingPanel()
		{
			if (GameHubBehaviour.Hub.Match.Kind == MatchData.MatchKind.Custom && (!ManagerController.Get<GroupManager>().IsUserInGroupOrIsOwner || ManagerController.Get<GroupManager>().GetSelfGroupStatus() == GroupStatus.Owner))
			{
				return;
			}
			this.ExitButtonCollider.enabled = true;
			this.Visible = true;
			this.TopPlayTimerLabel.gameObject.SetActive(true);
			GUIUtils.PlayAnimation(this.WindowAnimation, false, 1f, string.Empty);
		}

		public void AnimateHideMatchSearchingPanel()
		{
			if (GameHubBehaviour.Hub.Match.Kind == MatchData.MatchKind.Custom && (!ManagerController.Get<GroupManager>().IsUserInGroupOrIsOwner || ManagerController.Get<GroupManager>().GetSelfGroupStatus() == GroupStatus.Owner))
			{
				return;
			}
			if (!this.Visible)
			{
				return;
			}
			this.ExitButtonCollider.enabled = false;
			this.Visible = false;
			this.TopPlayTimerLabel.gameObject.SetActive(false);
			GUIUtils.PlayAnimation(this.WindowAnimation, true, this.AnimationHideSpeed, string.Empty);
		}

		public void OnClickedCancelBtn()
		{
			if (!this.Visible)
			{
				return;
			}
			if (this.MatchFound())
			{
				return;
			}
			MatchStatsGui.MainMenuGui.CancelMatchMaking();
			this.AnimateHideMatchSearchingPanel();
		}

		public void UpdateGameModePingLabel(GameModeTabs gameMode)
		{
			if (gameMode != GameModeTabs.CoopVsBots)
			{
				if (gameMode == GameModeTabs.Normal)
				{
					this.GameModeLabel.text = Language.Get("SELECTION_MODE_NORMAL", TranslationSheets.MainMenuGui);
				}
			}
			else
			{
				this.GameModeLabel.text = Language.Get("SELECTION_MODE_COOP", TranslationSheets.MainMenuGui);
			}
		}

		public void UpdateGameModePingSprite(RegionServerPing region)
		{
			switch (RegionPingUtils.GetPingStatus(region.Ping, GameHubBehaviour.Hub))
			{
			case RegionPingUtils.PingStatus.None:
				this.PingSprite.sprite2D = this.NoPing;
				break;
			case RegionPingUtils.PingStatus.High:
				this.PingSprite.sprite2D = this.HighPing;
				break;
			case RegionPingUtils.PingStatus.Average:
				this.PingSprite.sprite2D = this.MediumPing;
				break;
			case RegionPingUtils.PingStatus.Low:
				this.PingSprite.sprite2D = this.LowPing;
				break;
			}
		}

		public void OnClickedPlayBtn(GameModeTabs gameMode)
		{
			this.UpdateGameModePingLabel(gameMode);
			this.UpdateGameModePingSprite(SingletonMonoBehaviour<RegionController>.Instance.GetBestServerSaved());
		}

		private void RefreshOnlineStats()
		{
			this.NumberOfPlayers.text = Math.Max(0, this._NumberOfPlayers).ToString();
		}

		private void UpdateOnlinePlayers(int count)
		{
			this._NumberOfPlayers = count;
			this.RefreshOnlineStats();
		}

		public void SetStatusText(string status)
		{
			this._currentStatus = status;
		}

		public void UpdateRealTimeTxt(string status)
		{
			this.SetStatusText(status);
		}

		public void UpdateStatusTxt(string status)
		{
			this.SetStatusText(status);
		}

		public string GetStatus()
		{
			return this._currentStatus;
		}

		public void SetupPlaybutton()
		{
			this.UpdateStatusTxt(Language.Get("MatchSearching", "MainMenuGui"));
		}

		private bool IsInQueue()
		{
			return this.MMInterfaceState != MatchStatsGui.MatchmakingInterfaceState.None && this.MMInterfaceState != MatchStatsGui.MatchmakingInterfaceState.Play && !string.IsNullOrEmpty(MatchStatsGui.MainMenuGui.GetQueueName());
		}

		private bool ShouldClose(MatchStatsGui.MatchmakingInterfaceState newState)
		{
			return newState == MatchStatsGui.MatchmakingInterfaceState.OnTheLine && !GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish);
		}

		private void ChangeState(MatchStatsGui.MatchmakingInterfaceState newState)
		{
			SwordfishMatchmaking matchmaking = GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking;
			if ((this.MMInterfaceState == MatchStatsGui.MatchmakingInterfaceState.MatchMadeAsk || this.MMInterfaceState == MatchStatsGui.MatchmakingInterfaceState.OnTheLine) && newState == MatchStatsGui.MatchmakingInterfaceState.Play && !matchmaking.Connected && !matchmaking.WaitingForMatchResult && !matchmaking.LastFailed && MatchStatsGui.MainMenuGui.MatchAccept.acceptPanel.gameObject.activeSelf)
			{
				GameModesGUI.MatchBlocker.BlockPlayer();
			}
			switch (newState)
			{
			case MatchStatsGui.MatchmakingInterfaceState.None:
				MatchStatsGui.MainMenuGui.MatchAccept.HideAcceptanceWindow(false);
				this.TopPlayTimerGameObject.SetActive(false);
				this.TopPlayStatusGameObject.SetActive(false);
				this.TopPlayButtonStatusLabel.text = string.Empty;
				break;
			case MatchStatsGui.MatchmakingInterfaceState.WaitingServer:
				this.UpdateStatusTxt(Language.Get("MatchWaitingServerResponse", "MainMenuGui"));
				MatchStatsGui.MainMenuGui.MatchAccept.SetTitleText(this.GetStatus());
				MatchStatsGui.MainMenuGui.MatchAccept.WaitingServerStart();
				this.TopPlayTimerGameObject.SetActive(true);
				this.TopPlayStatusGameObject.SetActive(true);
				this.TopPlayButtonStatusLabel.text = this.GetStatus();
				break;
			case MatchStatsGui.MatchmakingInterfaceState.Play:
				this.SetupPlaybutton();
				MatchStatsGui.MainMenuGui.MatchAccept.HideAcceptanceWindow(true);
				this.SetStatusText(string.Empty);
				this.TopPlayButtonStatusLabel.text = this.GetStatus();
				this.TopPlayTimerLabel.text = string.Empty;
				this.TopPlayButtonStatusLabel.text = string.Empty;
				this.TopPlayTimerGameObject.SetActive(false);
				this.TopPlayStatusGameObject.SetActive(false);
				this.GameModeStartButton.text = this._cachedPlayBtnLocalized;
				break;
			case MatchStatsGui.MatchmakingInterfaceState.OnTheLine:
				this.SetStatusText(Language.Get("MatchSearching", "MainMenuGui"));
				if (this.ShouldClose(newState))
				{
					return;
				}
				MatchStatsGui.MainMenuGui.MatchAccept.HideAcceptanceWindow(true);
				this.TopPlayTimerGameObject.SetActive(true);
				this.TopPlayStatusGameObject.SetActive(true);
				this.TopPlayButtonStatusLabel.text = this.GetStatus();
				break;
			case MatchStatsGui.MatchmakingInterfaceState.MatchMadeAsk:
				this.SetStatusText(Language.Get("MatchQuestion", "MainMenuGui"));
				MatchStatsGui.MainMenuGui.MatchAccept.SetTitleText(this.GetStatus());
				MatchStatsGui.MainMenuGui.MatchAccept.ShowMatchConfirmation(matchmaking.GetNumBotsInMatchmakingMade(), matchmaking.GetMatchAcceptTimeout());
				GameHubBehaviour.Hub.GuiScripts.Esc.SetWindowVisibility(false);
				GameHubBehaviour.Hub.GuiScripts.ScreenResolution.HighlightWindow(true);
				this.TopPlayTimerGameObject.SetActive(true);
				this.TopPlayStatusGameObject.SetActive(false);
				MatchStatsGui.MainMenuGui.OnMatchFound();
				GameHubBehaviour.Hub.AnnouncerAudio.Play(AnnouncerVoiceOverType.MatchFound);
				break;
			case MatchStatsGui.MatchmakingInterfaceState.MatchMadeWaiting:
				this.SetStatusText(Language.Get("MatchWaitingForOtherPlayers", "MainMenuGui"));
				MatchStatsGui.MainMenuGui.MatchAccept.SetTitleText(this.GetStatus());
				MatchStatsGui.MainMenuGui.MatchAccept.WaitingServerStart();
				this.TopPlayTimerGameObject.SetActive(true);
				this.TopPlayStatusGameObject.SetActive(true);
				this.TopPlayButtonStatusLabel.text = this.GetStatus();
				break;
			}
		}

		public MatchStatsGui.MatchmakingInterfaceState MMInterfaceState
		{
			get
			{
				return this._mmInterfaceState;
			}
			set
			{
				if (this._mmInterfaceState != value)
				{
					this.ChangeState(value);
					this._mmInterfaceState = value;
				}
			}
		}

		public void OnMatchAccepted()
		{
			MatchStatsGui.MainMenuGui.SendMatchAccepted();
			this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.MatchMadeWaiting;
			this._lastConnectionState = SessionState.Closed;
		}

		public bool MatchFound()
		{
			return this.MMInterfaceState == MatchStatsGui.MatchmakingInterfaceState.WaitingServer || this.MMInterfaceState == MatchStatsGui.MatchmakingInterfaceState.MatchMadeAsk || this.MMInterfaceState == MatchStatsGui.MatchmakingInterfaceState.MatchMadeWaiting;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(MatchStatsGui));

		[Header("External Referencies")]
		public NetworkClient NetClient;

		[Header("Internal Referencies")]
		public UILabel NumberOfPlayers;

		public int _NumberOfPlayers;

		public UILabel GameModeStartButton;

		public UILabel TopPlayTimerLabel;

		public UILabel TopAveargeTimeLabel;

		public UILabel TopPlayButtonStatusLabel;

		public GameObject TopPlayTimerGameObject;

		public GameObject TopPlayStatusGameObject;

		public float _matchmakingStartTime = -1f;

		public BoxCollider ExitButtonCollider;

		[Header("Ping")]
		public UILabel GameModeLabel;

		public UI2DSprite PingSprite;

		public Sprite HighPing;

		public Sprite MediumPing;

		public Sprite LowPing;

		public Sprite NoPing;

		private SessionState _lastConnectionState;

		private string _cachedPlayBtnLocalized;

		[Header("[ANIMATIONS]")]
		[Range(0.5f, 5f)]
		public float AnimationHideSpeed = 2f;

		public Animation WindowAnimation;

		[Header("STATUS")]
		private string _currentStatus;

		public bool Visible;

		private int _lastTextUpdateTimeTotalSeconds = -1;

		public MatchStatsGui.MatchmakingInterfaceState _mmInterfaceState;

		public enum MatchmakingInterfaceState
		{
			Null,
			None,
			WaitingServer,
			Play,
			OnTheLine,
			MatchMadeAsk,
			MatchMadeWaiting,
			MatchConnecting
		}
	}
}
