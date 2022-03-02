using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Tournaments;
using HeavyMetalMachines.Tournaments.API;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX.PlotKids;
using Hoplon.Input;
using Hoplon.Localization.TranslationTable;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class MatchStatsGui : GameHubBehaviour
	{
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

		private void Awake()
		{
			this._mainMenuGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>();
			this._netClient = (NetworkClient)GameHubBehaviour.Hub.Net;
		}

		private void Start()
		{
			ManagerController.Get<GroupManager>().EvtInviteHandled += this.EvtInviteHandled;
			this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.None;
			this.ChangeState(MatchStatsGui.MatchmakingInterfaceState.None);
			SingletonMonoBehaviour<RegionController>.Instance.OnRegionServerChanged += this.OnOnRegionServerChanged;
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
			this.UpdateQueueKindIcon();
			IInputActiveDeviceChangeNotifier inputActiveDeviceChangeNotifier = this._diContainer.Resolve<IInputActiveDeviceChangeNotifier>();
			this._activeDeviceDisposable = ObservableExtensions.Subscribe<InputDevice>(Observable.Do<InputDevice>(inputActiveDeviceChangeNotifier.GetAndObserveActiveDeviceChange(), delegate(InputDevice device)
			{
				this.SetActiveCancelSearchButton(device != 3);
			}));
		}

		private void SetActiveCancelSearchButton(bool active)
		{
			this._cancelSearchButton.SetActive(active);
		}

		private void UpdateQueueKindIcon()
		{
			string currentQueueName = GameHubBehaviour.Hub.MatchmakingService.GetCurrentQueueName();
			string imageName;
			switch (currentQueueName)
			{
			case "RankedPSN":
			case "RankedXboxLive":
			case "Ranked":
				imageName = "search_ranked_icon";
				goto IL_CB;
			case "ProTournament":
			case "BeginnerTournament":
				imageName = "search_tournament_icon";
				goto IL_CB;
			}
			imageName = "search_pvp_icon";
			IL_CB:
			this._queueKindDynamicImage.SetImageName(imageName);
		}

		private void OnDestroy()
		{
			if (!SingletonMonoBehaviour<SocialController>.DoesInstanceExist())
			{
				return;
			}
			ManagerController.Get<GroupManager>().EvtInviteHandled -= this.EvtInviteHandled;
			SingletonMonoBehaviour<RegionController>.Instance.OnRegionServerChanged -= this.OnOnRegionServerChanged;
		}

		private void OnDisable()
		{
			if (this._activeDeviceDisposable != null)
			{
				this._activeDeviceDisposable.Dispose();
				this._activeDeviceDisposable = null;
			}
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
			string arg = Language.Get("QUEUE_AVG_TIME", TranslationContext.MainMenuGui);
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

		private void CheckAndChangeState()
		{
			SwordfishMatchmaking matchmaking = GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking;
			switch (this.MMInterfaceState)
			{
			case MatchStatsGui.MatchmakingInterfaceState.None:
				this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.Play;
				break;
			case MatchStatsGui.MatchmakingInterfaceState.WaitingServer:
				if (this._lastConnectionState != this._netClient.State)
				{
					string text = string.Empty;
					switch (this._netClient.State)
					{
					case SessionState.Closed:
					case SessionState.Lost:
						text = Language.Get("MatchClosed", TranslationContext.MainMenuGui);
						this.AnimateHideMatchSearchingPanel();
						this._waitingWindow.Hide(typeof(MatchStatsGui));
						this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.Play;
						break;
					case SessionState.Opening:
						text = Language.Get("MatchOpening", TranslationContext.MainMenuGui);
						break;
					case SessionState.Established:
						text = Language.Get("MatchEstablished", TranslationContext.MainMenuGui);
						break;
					}
					if (!string.IsNullOrEmpty(text))
					{
						this.UpdateStatusTxt(text);
						this._mainMenuGui.MatchAccept.SetTitleText(text);
					}
					this._lastConnectionState = this._netClient.State;
				}
				if (matchmaking.State == SwordfishMatchmaking.MatchmakingState.None && !matchmaking.Undefined)
				{
					this._waitingWindow.Hide(typeof(MatchStatsGui));
					this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.Play;
				}
				else if (matchmaking.State != SwordfishMatchmaking.MatchmakingState.Started && !GameHubBehaviour.Hub.User.IsReconnecting)
				{
					if (!matchmaking.Undefined)
					{
						this._waitingWindow.Hide(typeof(MatchStatsGui));
						this.MMInterfaceState = ((!matchmaking.Connected) ? MatchStatsGui.MatchmakingInterfaceState.Play : MatchStatsGui.MatchmakingInterfaceState.OnTheLine);
						if (matchmaking.LastFailed)
						{
							Guid confirmWindowGuid = Guid.NewGuid();
							ConfirmWindowProperties properties = new ConfirmWindowProperties
							{
								Guid = confirmWindowGuid,
								QuestionText = Language.Get("MatchFailedToConnect", TranslationContext.MainMenuGui),
								OkButtonText = Language.Get("Ok", TranslationContext.GUI),
								OnOk = delegate()
								{
									GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
									GameHubBehaviour.Hub.EndSession("MatchFailedToConnect");
								}
							};
							GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
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
				if (this._mainMenuGui.MatchAccept.Visible)
				{
					this._mainMenuGui.MatchAccept.HideAcceptanceWindow(false);
				}
				if (!matchmaking.Connected)
				{
					this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.Play;
				}
				else if (matchmaking.State == SwordfishMatchmaking.MatchmakingState.Made)
				{
					if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.IgnoreAutoTournamentJoin))
					{
						this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.MatchMadeAsk;
					}
					else
					{
						string currentQueueName = GameHubBehaviour.Hub.MatchmakingService.GetCurrentQueueName();
						if (currentQueueName != null)
						{
							if (MatchStatsGui.<>f__switch$map7 == null)
							{
								MatchStatsGui.<>f__switch$map7 = new Dictionary<string, int>(7)
								{
									{
										"Normal",
										0
									},
									{
										"Ranked",
										0
									},
									{
										"Novice",
										0
									},
									{
										"NormalPSN",
										0
									},
									{
										"NormalXboxLive",
										0
									},
									{
										"RankedPSN",
										0
									},
									{
										"RankedXboxLive",
										0
									}
								};
							}
							int num;
							if (MatchStatsGui.<>f__switch$map7.TryGetValue(currentQueueName, out num))
							{
								if (num == 0)
								{
									this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.MatchMadeAsk;
									break;
								}
							}
						}
						if (this.IsTournamentQueue(currentQueueName))
						{
							this._waitingWindow.Show(typeof(MatchStatsGui));
							this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.MatchMadeWaiting;
						}
					}
				}
				break;
			case MatchStatsGui.MatchmakingInterfaceState.MatchMadeAsk:
				if (!matchmaking.Connected)
				{
					this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.Play;
					this._mainMenuGui.OnClientDisconnectedFromMatchMaking();
				}
				else if (matchmaking.State == SwordfishMatchmaking.MatchmakingState.None)
				{
					this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.OnTheLine;
				}
				break;
			case MatchStatsGui.MatchmakingInterfaceState.MatchMadeWaiting:
				if (matchmaking.State == SwordfishMatchmaking.MatchmakingState.Started)
				{
					GameHubBehaviour.Hub.Swordfish.Msg.ConnectToMatch();
					this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.WaitingServer;
				}
				else if (!matchmaking.Connected && !matchmaking.WaitingForMatchResult)
				{
					this._waitingWindow.Hide(typeof(MatchStatsGui));
					this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.Play;
				}
				else if (matchmaking.State == SwordfishMatchmaking.MatchmakingState.None)
				{
					this._waitingWindow.Hide(typeof(MatchStatsGui));
					this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.OnTheLine;
				}
				break;
			}
		}

		private void FailedToConnect()
		{
			this.AnimateHideMatchSearchingPanel();
			this._waitingWindow.Hide(typeof(MatchStatsGui));
			this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.Play;
			this._mainMenuGui.MatchAccept.HideAcceptanceWindow(false);
		}

		public void DisconnectFromMatch()
		{
			this._mainMenuGui.ClearCurrentServer();
			this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.Play;
		}

		public void AnimateShowMatchSearchingPanel()
		{
			if (GameHubBehaviour.Hub.Match.Kind == 4 && (!ManagerController.Get<GroupManager>().IsUserInGroupOrIsOwner || ManagerController.Get<GroupManager>().GetSelfGroupStatus() == GroupStatus.Owner))
			{
				return;
			}
			MatchStatsGui.Log.Info(string.Format("Showing match search panel. MatchKind={0}", GameHubBehaviour.Hub.Match.Kind));
			this.ExitButtonCollider.enabled = true;
			this.Visible = true;
			this.TopPlayTimerLabel.gameObject.SetActive(true);
			GUIUtils.PlayAnimation(this.WindowAnimation, false, 1f, string.Empty);
			this.UpdateQueueKindIcon();
		}

		public void AnimateHideMatchSearchingPanel()
		{
			if (GameHubBehaviour.Hub.Match.Kind == 4 && (!ManagerController.Get<GroupManager>().IsUserInGroupOrIsOwner || ManagerController.Get<GroupManager>().GetSelfGroupStatus() == GroupStatus.Owner))
			{
				return;
			}
			SingletonMonoBehaviour<RegionController>.Instance.UpdateCurrentRegionOnSFServer(false);
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
			this._clientButtonBiLogger.LogButtonClick(ButtonName.MatchmakingCancelTopGui);
			MatchStatsGui.Log.DebugFormat("OnClickedCancelBtn MatchMaking state interface {0}", new object[]
			{
				this.MMInterfaceState
			});
			if (!this.Visible)
			{
				MatchStatsGui.Log.Debug("OnClickedCancelBtn is not visible");
				return;
			}
			if (this.MatchFound())
			{
				return;
			}
			IDialogPresenter dialogPresenter = this._diContainer.Resolve<IDialogPresenter>();
			ObservableExtensions.Subscribe<bool>(Observable.Do<bool>(Observable.Where<bool>(dialogPresenter.ShowQuestionWindow(this.GetCancelSearchQuestionConfig()), (bool accept) => accept), delegate(bool _)
			{
				this._mainMenuGui.CancelMatchMaking();
				this.AnimateHideMatchSearchingPanel();
			}));
		}

		private QuestionConfiguration GetCancelSearchQuestionConfig()
		{
			ILocalizeKey localizeKey = this._diContainer.Resolve<ILocalizeKey>();
			return new QuestionConfiguration
			{
				AcceptMessage = Language.Get("Yes", TranslationContext.GUI),
				DeclineMessage = Language.Get("No", TranslationContext.GUI),
				Message = localizeKey.Get("MAINMENU_EXIT_MATCH_QUESTION", TranslationContext.MainMenuGui)
			};
		}

		private void SetStatusText(string status)
		{
			this._currentStatus = status;
		}

		private void UpdateRealTimeTxt(string status)
		{
			this.SetStatusText(status);
		}

		private void UpdateStatusTxt(string status)
		{
			this.SetStatusText(status);
		}

		private void OnOnRegionServerChanged(RegionServerPing regionServerPing)
		{
			this.CurrentRegionLabel.text = Language.Get(regionServerPing.Region.RegionNameI18N, TranslationContext.Region);
		}

		private string GetStatus()
		{
			return this._currentStatus;
		}

		private void SetupPlaybutton()
		{
			this.UpdateStatusTxt(Language.Get("MatchSearching", TranslationContext.MainMenuGui));
		}

		private bool IsInQueue()
		{
			return this.MMInterfaceState != MatchStatsGui.MatchmakingInterfaceState.None && this.MMInterfaceState != MatchStatsGui.MatchmakingInterfaceState.Play && !string.IsNullOrEmpty(this._mainMenuGui.GetQueueName());
		}

		private bool ShouldClose(MatchStatsGui.MatchmakingInterfaceState newState)
		{
			return newState == MatchStatsGui.MatchmakingInterfaceState.OnTheLine && !GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish);
		}

		private void ChangeState(MatchStatsGui.MatchmakingInterfaceState newState)
		{
			SwordfishMatchmaking matchmaking = GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking;
			MatchStatsGui.Log.DebugFormat("Changing state from={0} to={1} mmConnected={2} mmUndefiend={3} mmState={4} mmWaitingForMatchResult={5} mmGroup={6} mmGroupId={7} mmLastFailed={8} mmMatchId={9}", new object[]
			{
				this.MMInterfaceState,
				newState,
				matchmaking.Connected,
				matchmaking.Undefined,
				matchmaking.State,
				matchmaking.WaitingForMatchResult,
				matchmaking.Group,
				matchmaking.GroupId,
				matchmaking.LastFailed,
				matchmaking.MatchId
			});
			if ((this.MMInterfaceState == MatchStatsGui.MatchmakingInterfaceState.MatchMadeAsk || this.MMInterfaceState == MatchStatsGui.MatchmakingInterfaceState.OnTheLine) && newState == MatchStatsGui.MatchmakingInterfaceState.Play && !matchmaking.Connected && !matchmaking.WaitingForMatchResult && !matchmaking.LastFailed && this._mainMenuGui.MatchAccept.acceptPanel.gameObject.activeSelf)
			{
				this._mainMenuGui.GameModesGui.BlockPlayer();
			}
			switch (newState)
			{
			case MatchStatsGui.MatchmakingInterfaceState.None:
				this._mainMenuGui.MatchAccept.HideAcceptanceWindow(false);
				this.TopPlayTimerGameObject.SetActive(false);
				break;
			case MatchStatsGui.MatchmakingInterfaceState.WaitingServer:
				this.UpdateStatusTxt(Language.Get("MatchWaitingServerResponse", TranslationContext.MainMenuGui));
				this._mainMenuGui.MatchAccept.SetTitleText(this.GetStatus());
				this._mainMenuGui.MatchAccept.WaitingServerStart();
				this.TopPlayTimerGameObject.SetActive(true);
				break;
			case MatchStatsGui.MatchmakingInterfaceState.Play:
				this.SetupPlaybutton();
				this._mainMenuGui.MatchAccept.HideAcceptanceWindow(true);
				this.SetStatusText(string.Empty);
				this.TopPlayTimerLabel.text = string.Empty;
				this.TopPlayTimerGameObject.SetActive(false);
				break;
			case MatchStatsGui.MatchmakingInterfaceState.OnTheLine:
				this.SetStatusText(Language.Get("MatchSearching", TranslationContext.MainMenuGui));
				if (this.ShouldClose(newState))
				{
					return;
				}
				this._mainMenuGui.MatchAccept.HideAcceptanceWindow(true);
				this.TopPlayTimerGameObject.SetActive(true);
				break;
			case MatchStatsGui.MatchmakingInterfaceState.MatchMadeAsk:
				this.OpenMatchStatsAccept(matchmaking);
				break;
			case MatchStatsGui.MatchmakingInterfaceState.MatchMadeWaiting:
			{
				string currentQueueName = GameHubBehaviour.Hub.MatchmakingService.GetCurrentQueueName();
				if (currentQueueName != null)
				{
					if (MatchStatsGui.<>f__switch$map8 == null)
					{
						MatchStatsGui.<>f__switch$map8 = new Dictionary<string, int>(7)
						{
							{
								"Novice",
								0
							},
							{
								"Normal",
								0
							},
							{
								"NormalPSN",
								0
							},
							{
								"NormalXboxLive",
								0
							},
							{
								"Ranked",
								0
							},
							{
								"RankedPSN",
								0
							},
							{
								"RankedXboxLive",
								0
							}
						};
					}
					int num;
					if (MatchStatsGui.<>f__switch$map8.TryGetValue(currentQueueName, out num))
					{
						if (num == 0)
						{
							this.SetStatusText(Language.Get("MatchWaitingForOtherPlayers", TranslationContext.MainMenuGui));
							this._mainMenuGui.MatchAccept.SetTitleText(this.GetStatus());
							this._mainMenuGui.MatchAccept.WaitingServerStart();
							this.TopPlayTimerGameObject.SetActive(true);
							break;
						}
					}
				}
				this.CheckTournamentQueue(currentQueueName);
				break;
			}
			}
		}

		private void CheckTournamentQueue(string queueName)
		{
			if (this.IsTournamentQueue(queueName))
			{
				GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.Accept(queueName);
				this._lastConnectionState = SessionState.Closed;
				GameHubBehaviour.Hub.AnnouncerAudio.Play(1);
			}
		}

		private bool IsTournamentQueue(string queueName)
		{
			TournamentTier[] all = this._getTournamentTier.GetAll();
			return all.Any((TournamentTier tier) => tier.QueuName == queueName);
		}

		private void OpenMatchStatsAccept(SwordfishMatchmaking mm)
		{
			this.SetStatusText(Language.Get("MatchQuestion", TranslationContext.MainMenuGui));
			this._mainMenuGui.MatchAccept.SetTitleText(this.GetStatus());
			this._mainMenuGui.MatchAccept.ShowMatchConfirmation(mm.GetNumBotsInMatchmakingMade(), mm.GetMatchAcceptTimeout(), mm.MatchId);
			GameHubBehaviour.Hub.GuiScripts.Esc.SetWindowVisibility(false);
			GameHubBehaviour.Hub.GuiScripts.ScreenResolution.HighlightWindow(true);
			this.TopPlayTimerGameObject.SetActive(true);
			this._mainMenuGui.OnMatchFound();
			GameHubBehaviour.Hub.AnnouncerAudio.Play(1);
		}

		public void OnMatchAccepted(string queueName)
		{
			this._mainMenuGui.SendMatchAccepted(queueName);
			this.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.MatchMadeWaiting;
			this._lastConnectionState = SessionState.Closed;
		}

		public bool MatchFound()
		{
			return this.MMInterfaceState == MatchStatsGui.MatchmakingInterfaceState.WaitingServer || this.MMInterfaceState == MatchStatsGui.MatchmakingInterfaceState.MatchMadeAsk || this.MMInterfaceState == MatchStatsGui.MatchmakingInterfaceState.MatchMadeWaiting;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(MatchStatsGui));

		[Header("Internal Referencies")]
		public UILabel NumberOfPlayers;

		public int _NumberOfPlayers;

		[SerializeField]
		private UILabel GameModeStartButton;

		[SerializeField]
		private UILabel TopPlayTimerLabel;

		[SerializeField]
		private UILabel TopAveargeTimeLabel;

		[SerializeField]
		private GameObject TopPlayTimerGameObject;

		[SerializeField]
		private BoxCollider ExitButtonCollider;

		[SerializeField]
		private Animation WindowAnimation;

		[SerializeField]
		private NGuiDynamicImage _queueKindDynamicImage;

		[SerializeField]
		private UILabel CurrentRegionLabel;

		[SerializeField]
		private NGuiButton _cancelSearchButton;

		[Inject]
		private IWaitingWindow _waitingWindow;

		[Inject]
		private DiContainer _diContainer;

		private IDisposable _activeDeviceDisposable;

		[Inject]
		private IClientButtonBILogger _clientButtonBiLogger;

		[Inject]
		private IGetTournamentTier _getTournamentTier;

		[Header("[ANIMATIONS]")]
		[Range(0.5f, 5f)]
		public float AnimationHideSpeed = 2f;

		public bool Visible;

		private MainMenuGui _mainMenuGui;

		private NetworkClient _netClient;

		private int _lastTextUpdateTimeTotalSeconds = -1;

		private float _matchmakingStartTime = -1f;

		private string _currentStatus;

		private SessionState _lastConnectionState;

		private MatchStatsGui.MatchmakingInterfaceState _mmInterfaceState;

		private string _cachedPlayBtnLocalized;

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
