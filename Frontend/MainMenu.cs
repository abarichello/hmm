using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI;
using ClientAPI.MessageHub;
using ClientAPI.Objects;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Battlepass;
using HeavyMetalMachines.CharacterSelection.Rotation;
using HeavyMetalMachines.CompetitiveMode;
using HeavyMetalMachines.Crossplay;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.DLC;
using HeavyMetalMachines.Infra.Quiz;
using HeavyMetalMachines.LegacyStorage;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.MatchMaking;
using HeavyMetalMachines.ParentalControl;
using HeavyMetalMachines.ParentalControl.Restrictions;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.GenericConfirmWindow;
using HeavyMetalMachines.Social.FriendDataUpdater.Business;
using HeavyMetalMachines.Social.Friends.Business;
using HeavyMetalMachines.Social.Friends.Business.BlockedPlayers;
using HeavyMetalMachines.Social.Friends.Business.RecentPlayers;
using HeavyMetalMachines.Social.Friends.Business.Teams;
using HeavyMetalMachines.Social.Friends.Models;
using HeavyMetalMachines.Social.Friends.PlatformRecommended;
using HeavyMetalMachines.Social.Groups.Business;
using HeavyMetalMachines.Store.Business;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.VoiceChat.Business;
using Hoplon.Localization.TranslationTable;
using Hoplon.Serialization;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenu : GameState
	{
		private IObservable<Unit> FetchAndStoreParentalControl()
		{
			return Observable.Defer<Unit>(delegate()
			{
				List<IPlayer> players = new List<IPlayer>();
				IFetchAndStorePlayersParentalControlInfo fetchAndStorePlayersParentalControlInfo = this._diContainer.Resolve<IFetchAndStorePlayersParentalControlInfo>();
				return Observable.ContinueWith<BlockedPlayer[], Unit>(Observable.Do<BlockedPlayer[]>(Observable.ContinueWith<FriendsTeamMember[], BlockedPlayer[]>(Observable.Do<FriendsTeamMember[]>(Observable.ContinueWith<RecentPlayer[], FriendsTeamMember[]>(Observable.Do<RecentPlayer[]>(Observable.ContinueWith<PlatformRecommendedFriend[], RecentPlayer[]>(Observable.Do<PlatformRecommendedFriend[]>(Observable.ContinueWith<Friend[], PlatformRecommendedFriend[]>(Observable.Do<Friend[]>(Observable.Return<Friend[]>(this._getFriends.Get()), delegate(Friend[] friends)
				{
					players.AddRange(friends);
				}), this._getPlatformRecommendedFriends.Get()), delegate(PlatformRecommendedFriend[] recommendedFriends)
				{
					players.AddRange(recommendedFriends);
				}), this._getRecentPlayers.Get()), delegate(RecentPlayer[] recentPlayers)
				{
					players.AddRange(recentPlayers);
				}), this._getTeamMembers.Get()), delegate(FriendsTeamMember[] teamMembers)
				{
					players.AddRange(teamMembers);
				}), this._getBlockedPlayers.Get()), delegate(BlockedPlayer[] blockedPlayers)
				{
					players.AddRange(blockedPlayers);
				}), (BlockedPlayer[] _) => MainMenu.FetchAndStore(fetchAndStorePlayersParentalControlInfo, players));
			});
		}

		private static IObservable<Unit> FetchAndStore(IFetchAndStorePlayersParentalControlInfo fetchAndStorePlayersParentalControlInfo, List<IPlayer> players)
		{
			return fetchAndStorePlayersParentalControlInfo.FetchAndStore((from p in players
			select new PlayerIdentification
			{
				PlayerId = p.PlayerId,
				UniversalId = p.UniversalId
			}).ToArray<PlayerIdentification>());
		}

		private IObservable<Unit> SetCrossPlayDisabledIfNeeded()
		{
			return Observable.Defer<Unit>(delegate()
			{
				if (this._getCrossPlayRestrictionIsEnabled.Get())
				{
					return this._setAndResolveCrossplay.SetAndResolve(false);
				}
				return Observable.ReturnUnit();
			});
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event Action PlayerReloadedEvent;

		private void OnPlayerReloaded()
		{
			if (MainMenu.PlayerReloadedEvent != null)
			{
				MainMenu.PlayerReloadedEvent();
			}
		}

		protected override IObservable<Unit> OnStateEnabledAsync()
		{
			return this.FetchInitialData();
		}

		private IObservable<Unit> FetchInitialData()
		{
			if (this._skipSwordfish)
			{
				return this.FetchStoreItems();
			}
			return Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(this.InitializeCompetitiveMode(), (Unit _) => this.FetchAndStoreParentalControl()), (Unit _) => this.SetCrossPlayDisabledIfNeeded()), (Unit _) => this.LoadMainMenuData()), delegate(Unit _)
			{
				this.InitializeFriendDataUpdater();
			}), delegate(Unit _)
			{
				this.InitializeGroupCrossplayStorage();
			}), delegate(Unit _)
			{
				this.InitializeDlcChangeObservation();
			});
		}

		private void InitializeGroupCrossplayStorage()
		{
			IGroupCrossplayStorage groupCrossplayStorage = this._diContainer.Resolve<IGroupCrossplayStorage>();
			groupCrossplayStorage.Initialize();
		}

		private void InitializeFriendDataUpdater()
		{
			this._localFriendDataUpdater = this._diContainer.ResolveId<ILocalFriendDataUpdater>(0);
			this._friendDataUpdaterDisposable = ObservableExtensions.Subscribe<Unit>(this._localFriendDataUpdater.ExecuteIndefinitely(), delegate(Unit onNext)
			{
				MainMenu.Log.Debug("SwordfishLocalFriendDataService onNext");
			}, delegate(Exception onError)
			{
				MainMenu.Log.ErrorFormat("SwordfishLocalFriendDataService Error={0}", new object[]
				{
					onError
				});
			});
		}

		private void InitializeDlcChangeObservation()
		{
			this._observeDlcChangeDisposable = ObservableExtensions.Subscribe<Unit>(Observable.DoOnError<Unit>(Observable.Do<Unit>(Observable.SelectMany<Unit, Unit>(Observable.Do<Unit>(this._diContainer.Resolve<IObserveDlcChange>().Observe(), delegate(Unit _)
			{
				this._waitingWindow.Show(false, typeof(IObserveDlcChange));
			}), (Unit _) => this.LoadMainMenuData()), delegate(Unit _)
			{
				this._waitingWindow.Hide(typeof(IObserveDlcChange));
			}), delegate(Exception _)
			{
				this._waitingWindow.Hide(typeof(IObserveDlcChange));
			}), delegate(Unit onNext)
			{
				MainMenu.Log.Debug("ObserveDlcChange onNext");
			}, delegate(Exception onError)
			{
				MainMenu.Log.ErrorFormat("ObserveDlcChange Error={0}", new object[]
				{
					onError
				});
			});
		}

		private IObservable<Unit> LoadMainMenuData()
		{
			if (this._requestedMainMenuData)
			{
				return Observable.ReturnUnit();
			}
			this._requestedMainMenuData = true;
			this._mainMenuData = null;
			this._swordfishWsService = this._diContainer.Resolve<ISwordfishWsService>();
			this._customWs = this._diContainer.Resolve<ICustomWS>();
			return Observable.AsUnitObservable<MainMenuData>(Observable.Do<MainMenuData>(Observable.Do<MainMenuData>(Observable.Catch<MainMenuData, Exception>(Observable.SelectMany<Unit, MainMenuData>(Observable.ContinueWith<Unit, Unit>(this.FetchStoreItems(), this.FetchBattlepassConfiguration()), this.CallGetMainMenuData()), (Exception exception) => this.HandleGetMainMenuDataError(exception)), delegate(MainMenuData _)
			{
				this._localPlayerTotalLevelStorage.RefreshTotalLevel();
			}), delegate(MainMenuData menuData)
			{
				this._requestedMainMenuData = false;
				this._mainMenuData = menuData;
			}));
		}

		protected override void OnStateEnabled()
		{
			MainMenu.Log.Debug("OnStateEnabled");
			this._hub = GameHubBehaviour.Hub;
			this._isMainMenuEnable = true;
			UIRoot.SetUiState(UIRoot.UiState.MainMenu);
			this._skipSwordfish = GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false);
			if (this._skipSwordfish)
			{
				return;
			}
			this._hub.Swordfish.Log.BILogClient(4, true);
			this._hub.Swordfish.Connection.RegisterConnectionMonitoring();
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.ShowWelcomePage) && !GameHubBehaviour.Hub.TutorialHub.TutorialControllerInstance.HasPlayerDoneTutorial())
			{
				this.ShowWelcomePage();
			}
			this._observeMatchMakingLostConnectionDisposable = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(Observable.First<Unit>(this._diContainer.Resolve<IObserveMatchMakingLostConnection>().Observe()), delegate(Unit _)
			{
				this.ShowConnectionLost("MatchMakingConnection Lost");
			}));
		}

		public void ShowConnectionLost(string reason)
		{
			MainMenu.Log.Warn("CONNECTION LOST");
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				IsStackable = false,
				Guid = confirmWindowGuid,
				QuestionText = Language.Get("LostMessageHubConnection", TranslationContext.GUI),
				OkButtonText = Language.Get("Ok", TranslationContext.GUI),
				OnOk = delegate()
				{
					this._hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
					this._hub.EndSession(reason);
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		private void ShowWelcomePage()
		{
			string value = GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.WelcomePageURL);
			if (0 < value.Length)
			{
				try
				{
					string text = string.Format(value, Language.CurrentLanguage, this._hub.User.UserSF.UniversalID);
					this._hub.Swordfish.Log.BILogClientMsg(24, text, true);
					using (StreamWriter streamWriter = new StreamWriter("welcome"))
					{
						streamWriter.Write(text);
					}
				}
				catch (Exception ex)
				{
					MainMenu.Log.WarnFormat("Write to file failed. Welcome page might not open. Exception: {0}", new object[]
					{
						ex
					});
				}
			}
		}

		protected override void OnStateDisabled()
		{
			MainMenu.Log.Debug("OnStateDisabled");
			this._hub.Swordfish.Log.BILogClient(6, true);
			this._isMainMenuEnable = false;
			this._hub.Swordfish.Connection.DeregisterConnectionMonitoring();
			ObservableExtensions.Subscribe<QuizBag>(this.GetQuizTypeForPlayer());
			if (this.MainMenuGui != null)
			{
				this.MainMenuGui = null;
			}
			if (this._friendDataUpdaterDisposable != null)
			{
				this._friendDataUpdaterDisposable.Dispose();
				this._friendDataUpdaterDisposable = null;
			}
			if (this._observeDlcChangeDisposable != null)
			{
				this._observeDlcChangeDisposable.Dispose();
				this._observeDlcChangeDisposable = null;
			}
			if (this._observeMatchMakingLostConnectionDisposable != null)
			{
				this._observeMatchMakingLostConnectionDisposable.Dispose();
				this._observeMatchMakingLostConnectionDisposable = null;
			}
			GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ForceCloseWaitingWindow();
		}

		private IObservable<MainMenuData> HandleGetMainMenuDataError(Exception exception)
		{
			this._requestedMainMenuData = false;
			MainMenu.Log.ErrorFormat("Failed to get MainMenuData. Ex: {0}", new object[]
			{
				exception
			});
			MainMenuData mainMenuData = null;
			return Observable.ContinueWith<Unit, MainMenuData>(this.ShowLostConnectionWindow("GetMainMenuData failed"), Observable.Return<MainMenuData>(mainMenuData));
		}

		private IObservable<Unit> ShowLostConnectionWindow(string eventName)
		{
			DialogConfiguration dialogConfiguration = new DialogConfiguration
			{
				Message = this._localizeKey.Get("LostMessageHubConnection", TranslationContext.GUI)
			};
			return Observable.Do<Unit>(this._genericConfirmWindowPresenter.Show(dialogConfiguration), delegate(Unit _)
			{
				GameHubBehaviour.Hub.EndSession(eventName);
			});
		}

		protected override IObservable<Unit> OnMyLevelLoadedAsync()
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				if (this._skipSwordfish)
				{
					this.GetMainMenuDataSkipSwordfish();
					this._mainMenuData = null;
					return;
				}
				if (this._mainMenuData != null)
				{
					this.OnGetMainMenuDataSuccess(this._mainMenuData);
					this._mainMenuData = null;
				}
			});
		}

		protected override void OnMyLevelLoaded()
		{
			MainMenu.Log.Debug("OnMyLevelLoaded");
			this.MainMenuGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>();
			this._hub.User.ReloadCharactersData(this, null);
		}

		public IObservable<MainMenuData> GetMainMenuData()
		{
			if (this._requestedMainMenuData)
			{
				return Observable.Return<MainMenuData>(null);
			}
			this._requestedMainMenuData = true;
			return Observable.DoOnError<MainMenuData>(Observable.Do<MainMenuData>(Observable.Do<MainMenuData>(Observable.Catch<MainMenuData, Exception>(Observable.ContinueWith<Unit, MainMenuData>(Observable.ContinueWith<Unit, Unit>(this.FetchStoreItems(), this.FetchBattlepassConfiguration()), this.CallGetMainMenuData()), new Func<Exception, IObservable<MainMenuData>>(this.HandleGetMainMenuDataError)), new Action<MainMenuData>(this.OnGetMainMenuDataSuccess)), delegate(MainMenuData _)
			{
				this._localPlayerTotalLevelStorage.RefreshTotalLevel();
			}), new Action<Exception>(this.OnGetMainMenuDataFailure));
		}

		private IObservable<MainMenuData> CallGetMainMenuData()
		{
			IObservable<string> observable = this._swordfishWsService.Execute<string>(delegate(SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
			{
				this._customWs.ExecuteCustomWSWithReturn(null, "GetMainMenuData", string.Empty, callback, errorCallback);
			});
			if (MainMenu.<>f__mg$cache0 == null)
			{
				MainMenu.<>f__mg$cache0 = new Func<string, MainMenuData>(JsonSerializeable<MainMenuData>.Deserialize);
			}
			return Observable.Select<string, MainMenuData>(observable, MainMenu.<>f__mg$cache0);
		}

		private IObservable<Unit> FetchBattlepassConfiguration()
		{
			return this._diContainer.Resolve<IGetBattlepassSeasonProvider>().FetchSeason();
		}

		private IObservable<Unit> FetchStoreItems()
		{
			return Observable.DoOnError<Unit>(this._diContainer.Resolve<IStoreBusinessFactory>().CreateRefreshStoreItemStorage().RefreshAllItems(), delegate(Exception exception)
			{
				MainMenu.Log.ErrorFormat("Could not refresh store item types. {0}", new object[]
				{
					exception
				});
			});
		}

		private void GetMainMenuDataSkipSwordfish()
		{
			this.MainMenuGui.OnGetMainMenuDataSuccess(null);
		}

		private void OnGetMainMenuDataFailure(Exception e)
		{
			this._requestedMainMenuData = false;
			MainMenu.Log.Error("Failed to get MainMenuData", e);
			Debug.LogError("Failed to get MainMenuData - error: " + e);
		}

		private void OnGetMainMenuDataSuccess(MainMenuData receivedMainMenuData)
		{
			this._requestedMainMenuData = false;
			this._diContainer.Resolve<MainMenuDataStorage>().LatestMainMenuData = receivedMainMenuData;
			MainMenu.Log.Info("Received MainMenuData");
			if (!this._isMainMenuEnable)
			{
				MainMenu.Log.WarnFormat("Player is no longer in MainMenu", new object[0]);
				return;
			}
			this._matchHistoryStorage.SetMatchHistory(receivedMainMenuData);
			this._rotationWeekStorage.Set(receivedMainMenuData.CurrentRotationWeek);
			this._hub.User.SetPlayer(receivedMainMenuData.Player);
			this._hub.User.SetBattlepassProgress(receivedMainMenuData.BattlepassProgressString);
			this._hub.Store.SetBalance(receivedMainMenuData.SoftCurrency, receivedMainMenuData.HardCurrency);
			this._hub.User.Inventory.SetAllReloadedItems(receivedMainMenuData.PlayerInventories, receivedMainMenuData.PlayerItems);
			this._hub.User.Inventory.HasNewItems = (receivedMainMenuData.NewItemsCount > 0);
			this._hub.User.Inventory.RefreshPlayerCustomizations();
			this.OnPlayerReloaded();
			this.SyncServerTime();
			this._hub.Options.Game.CheckInitialConfig();
			this.SetClockServerUtcOffset(receivedMainMenuData);
			this.MainMenuGui.OnGetMainMenuDataSuccess(receivedMainMenuData);
			this.EnableOrDisableCrossplay(receivedMainMenuData.PsnCrossplayEnabled);
			this.CheckForParentalControl();
			MainMenu.Log.WarnFormat("Finished Received MainMenuData", new object[0]);
		}

		private void CheckForParentalControl()
		{
			IVoiceChatRestriction voiceChatRestriction = this._diContainer.Resolve<IVoiceChatRestriction>();
			if (voiceChatRestriction.IsGloballyEnabled())
			{
				this._voiceChatPreferences.TeamStatus = 0;
			}
		}

		private void EnableOrDisableCrossplay(bool crossplayEnabled)
		{
			this._setCrossplay.Set(crossplayEnabled);
		}

		public void SyncServerTime()
		{
			StoreCustomWS.SyncServerTime(delegate(object state, string s)
			{
				MainMenu.Log.Debug("SyncServerTime done");
				if (this.MainMenuGui != null)
				{
					this.MainMenuGui.OnAllItemsReload();
				}
				else
				{
					MainMenu.Log.Warn("MainMenuGui is null on SyncServerTime success");
				}
			}, delegate(object state, Exception exception)
			{
				MainMenu.Log.ErrorFormat("Error syncing server time: {0}", new object[]
				{
					exception.Message
				});
			});
		}

		private void OnMsgHubDisconnected(object sender, DisconnectionReasonWrapper e)
		{
			Guid guid = Guid.NewGuid();
			ConfirmWindowProperties confirmWindowProperties = new ConfirmWindowProperties();
			confirmWindowProperties.Guid = guid;
			confirmWindowProperties.QuestionText = Language.Get("LostMessageHubConnection", TranslationContext.GUI);
			confirmWindowProperties.OkButtonText = Language.Get("Ok", TranslationContext.GUI);
			confirmWindowProperties.OnOk = delegate()
			{
				GameHubBehaviour.Hub.EndSession("MainMenu: ClientApi.hubClient.Disconnected");
			};
			ConfirmWindowProperties properties = confirmWindowProperties;
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		private void SetClockServerUtcOffset(MainMenuData mainMenuData)
		{
			long num;
			if (long.TryParse(mainMenuData.UtcNowTicksString, out num))
			{
				this._hub.Clock.ServerUtcOffset = new TimeSpan(num - DateTime.UtcNow.Ticks);
			}
		}

		private IObservable<Unit> InitializeCompetitiveMode()
		{
			return this._diContainer.Resolve<IInitializeCompetitiveMode>().Initialize();
		}

		public IObservable<QuizBag> GetQuizTypeForPlayer()
		{
			return Observable.DoOnError<QuizBag>(Observable.Do<QuizBag>(Observable.Select<NetResult, QuizBag>(this.GetPlayerEligibleForRookieQuizFromCustomWs(), (NetResult netResult) => (QuizBag)((JsonSerializeable<!0>)netResult.Msg)), new Action<QuizBag>(this.OnPlayerEligibleSuccess)), new Action<Exception>(this.OnGetPlayerEligibleFailure));
		}

		private void OnGetPlayerEligibleFailure(Exception obj)
		{
			MainMenu.Log.ErrorFormat("Failed to get PlayerEligibleForRookieQuiz. Exception: {0}", new object[]
			{
				obj
			});
		}

		private void OnPlayerEligibleSuccess(QuizBag quizBag)
		{
			if (quizBag.IsPlayerEligibleForRookieQuiz)
			{
				this._quizUrlFileProvider.TryCreateQuizUrlFile(this._hub.Swordfish.Msg.ClientMatchId);
				return;
			}
			this._quizUrlFileProvider.TryDeleteQuizUrlFile();
		}

		private IObservable<NetResult> GetPlayerEligibleForRookieQuizFromCustomWs()
		{
			return SwordfishObservable.FromStringSwordfishCall<NetResult>(delegate(SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
			{
				PlayerCustomWS.IsPlayerEligibleForRookieQuiz(GameHubBehaviour.Hub.User.UniversalId, onSuccess, onError);
			});
		}

		private IEnumerator SkipSwordfishStartMatch()
		{
			yield return new WaitForSeconds(1f);
			GameHubBehaviour.Hub.User.ConnectToServer(false, delegate
			{
				GameHubBehaviour.Hub.State.GotoState(this, false);
			}, null);
			yield break;
		}

		public bool SearchForAMatch(string queueName)
		{
			MainMenu.Log.DebugFormat("SearchForAMatch {0}", new object[]
			{
				queueName
			});
			if (this._skipSwordfish)
			{
				base.StartCoroutine(this.SkipSwordfishStartMatch());
			}
			else
			{
				SwordfishMatchmaking matchmaking = this._hub.Swordfish.Msg.Matchmaking;
				if (this._hub.User.PartyId == Guid.Empty)
				{
					matchmaking.StartMatch(queueName, new Action(this.SearchForAMatchFailed));
				}
				else
				{
					List<string> list = new List<string>();
					for (int i = 0; i < ManagerController.Get<GroupManager>().GroupMembersSortedList.Count; i++)
					{
						GroupMember groupMember = ManagerController.Get<GroupManager>().GroupMembersSortedList[i];
						if (!(groupMember.UniversalID == this._hub.User.UniversalId))
						{
							list.Add(groupMember.UniversalID);
						}
					}
					matchmaking.StartGroupMatch(this._hub.User.PartyId, list.ToArray(), queueName, new Action(this.SearchForAMatchFailed));
				}
			}
			return false;
		}

		private void SearchForAMatchFailed()
		{
			ObservableExtensions.Subscribe<Unit>(this.ShowLostConnectionWindow("MainMenu.SearchForAMatchFailed"));
		}

		public void CancelMatchMaking()
		{
			this._hub.Swordfish.Msg.Matchmaking.StopSearch();
		}

		public void GoToMatch()
		{
			this._hub.Swordfish.Msg.ConnectToMatch();
		}

		public void SendMatchAccepted(string queueName)
		{
			this._hub.Swordfish.Msg.Matchmaking.Accept(queueName);
		}

		public void SendRejectMatch()
		{
			this._hub.Swordfish.Msg.Matchmaking.Decline();
		}

		public void ClearCurrentServer()
		{
			PlayerCustomWS.ClearCurrentServer(delegate(object x, string res)
			{
				MainMenu.Log.DebugFormat("Server cleared, result={0}", new object[]
				{
					res
				});
			}, delegate(object x, Exception ex)
			{
				MainMenu.Log.Fatal("Error clearing server from bag.", ex);
			});
		}

		public static readonly BitLogger Log = new BitLogger(typeof(MainMenu));

		private HMMHub _hub;

		private MainMenuGui MainMenuGui;

		public Item ItemState;

		public CharacterWorkshop CharacterWorkshopState;

		public Options OptionsState;

		private bool _skipSwordfish;

		private bool _callbacksInstalled;

		private bool _isMainMenuEnable;

		private bool _requestedMainMenuData;

		[Inject]
		private DiContainer _diContainer;

		[Inject]
		private IQuizUrlFileProvider _quizUrlFileProvider;

		[Inject]
		private IRotationWeekStorage _rotationWeekStorage;

		[Inject]
		private IMatchHistoryStorage _matchHistoryStorage;

		[Inject]
		private ILocalPlayerTotalLevelStorage _localPlayerTotalLevelStorage;

		[Inject]
		private ISetCrossplay _setCrossplay;

		[Inject]
		private ISetAndResolveCrossplay _setAndResolveCrossplay;

		[Inject]
		private IVoiceChatPreferences _voiceChatPreferences;

		private ISwordfishWsService _swordfishWsService;

		private ILocalFriendDataUpdater _localFriendDataUpdater;

		private IDisposable _friendDataUpdaterDisposable;

		private IDisposable _observeDlcChangeDisposable;

		private IDisposable _observeMatchMakingLostConnectionDisposable;

		private ICustomWS _customWs;

		[Inject]
		private ILocalizeKey _localizeKey;

		[Inject]
		private IGenericConfirmWindowPresenter _genericConfirmWindowPresenter;

		[Inject]
		private IGetFriends _getFriends;

		[Inject]
		private IGetPlatformRecommendedFriends _getPlatformRecommendedFriends;

		[Inject]
		private IGetRecentPlayers _getRecentPlayers;

		[Inject]
		private IGetTeamMembers _getTeamMembers;

		[Inject]
		private IGetBlockedPlayers _getBlockedPlayers;

		[Inject]
		private IGetCrossPlayRestrictionIsEnabled _getCrossPlayRestrictionIsEnabled;

		[Inject]
		private IWaitingWindow _waitingWindow;

		private MainMenuData _mainMenuData;

		[CompilerGenerated]
		private static Func<string, MainMenuData> <>f__mg$cache0;
	}
}
