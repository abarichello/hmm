using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI;
using ClientAPI.Objects;
using HeavyMetalMachines.CharacterSelection.Client;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.ScriptableObjects;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Player;
using HeavyMetalMachines.ToggleableFeatures;
using Hoplon.Localization.TranslationTable;
using Hoplon.Serialization;
using Hoplon.ToggleableFeatures;
using Pocketverse;
using Pocketverse.MuralContext;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines
{
	public class UserInfo : GameHubBehaviour, ICleanupListener
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event Action<PlayerBag> EvtPlayerBagUpdated;

		public PlayerBag Bag
		{
			get
			{
				return this._bag;
			}
			set
			{
				this._bag = value;
				if (UserInfo.EvtPlayerBagUpdated != null)
				{
					UserInfo.EvtPlayerBagUpdated(this._bag);
				}
			}
		}

		public bool IsClusterAuth { get; private set; }

		public string UniversalId { get; set; }

		public bool IsUniversalIdLocalPlayer(string universalId)
		{
			return string.Equals(this.UniversalId, universalId);
		}

		public Wallet Wallet
		{
			get
			{
				return this._wallet;
			}
		}

		public Guid PartyId
		{
			get
			{
				return ManagerController.Get<GroupManager>().CurrentGroupID;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event UserInfo.UserUniversalIdTaken OnUserUniversalIdTaken;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event UserInfo.MatchDataReceived OnMatchDataReceived;

		public void SetLoginData(LoginData loginData)
		{
			this.StoreAndInitializeRemotelyEnabledFeatures(loginData);
			this.UserSF = loginData.MyUser;
			this.UniversalId = loginData.MyUser.UniversalID;
			this.SetPlayer(loginData.MyPlayer);
			this.Characters = loginData.MyCharacters;
			this._wallet = loginData.MyWallet;
			this.Name = loginData.MyUser.Login;
			UserInfo.Log.InfoFormat("UserInfo: {0}-Player={1} UserId={2} UniversalId={3} Bag={4}", new object[]
			{
				this.PlayerSF.Id,
				this.PlayerSF.Name,
				this.UserSF.Id,
				this.UniversalId,
				this.PlayerSF.Bag
			});
			if (this.CheckAndUpdateIsFirstLogin())
			{
				GameHubBehaviour.Hub.Swordfish.Log.BILogClient(23, true);
			}
			ControlOptions.LogBICurrentLanguage();
			this.IsClusterAuth = loginData.MyUser.IsClusterAuth;
			if (this.OnUserUniversalIdTaken != null)
			{
				this.OnUserUniversalIdTaken();
			}
			if (this.Inventory == null)
			{
				this.Inventory = (base.gameObject.GetComponent<PlayerInventory>() ?? base.gameObject.AddComponent<PlayerInventory>());
			}
			this.Inventory.SetAllReloadedItems(loginData.InventoriesSf, null);
			GameHubBehaviour.Hub.InventoryColletion.SetAllItemTypes(loginData.ItemTypesSf);
		}

		private void StoreAndInitializeRemotelyEnabledFeatures(LoginData loginData)
		{
			this._remotelyEnabledFeaturesNamesStorage.FeaturesNames = loginData.RemotelyEnabledFeaturesNames;
			this._initializeRemotelyToggleableFeatures.Initialize();
		}

		private bool CheckAndUpdateIsFirstLogin()
		{
			if (GameHubBehaviour.Hub.Swordfish.Connection.IsFirstLogin)
			{
				return true;
			}
			PlayerBag playerBag = (PlayerBag)this.PlayerSF.Bag;
			bool flag = playerBag.Level == 0 && playerBag.Xp == 0 && !playerBag.HasDoneTutorial;
			GameHubBehaviour.Hub.Swordfish.Connection.SetIsFirstLogin(flag);
			return flag;
		}

		public void OnCleanup(CleanupMessage msg)
		{
			this.IsReconnecting = false;
			this.IsNarrator = false;
			if (SingletonMonoBehaviour<SpectatorController>.DoesInstanceExist())
			{
				SingletonMonoBehaviour<SpectatorController>.Instance.CurrentSpectatorRole = SpectatorRole.None;
			}
		}

		public void SetPlayer(Player player)
		{
			this.PlayerSF = player;
			this.Bag = (PlayerBag)player.Bag;
		}

		public void SetBattlepassProgress(string battlepassProgressBag)
		{
			this._battlepassProgress.UpdateBattlepassProgress((BattlepassProgress)battlepassProgressBag);
		}

		public void SetTestBattlepassProgressScriptableObject(BattlepassProgressScriptableObject progress)
		{
			this._battlepassProgress = progress;
		}

		public void ReloadPlayer(Action afterLoad, Action failAction)
		{
			if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
			{
				CallbackHolder callbackHolder = new CallbackHolder
				{
					SucessCallBack = afterLoad,
					FailureCallBack = failAction
				};
				GameHubBehaviour.Hub.ClientApi.user.GetMyPlayer(callbackHolder, new SwordfishClientApi.ParameterizedCallback<Player>(this.OnGetPlayerSuccessful), new SwordfishClientApi.ErrorCallback(this.OnGetPlayerFailure));
			}
		}

		public int GetTotalPlayerLevel()
		{
			BattlepassConfig battlepass = GameHubBehaviour.Hub.SharedConfigs.Battlepass;
			BattlepassProgress progress = this._battlepassProgress.Progress;
			return GameHubBehaviour.Hub.User.Bag.AccountLevel(progress, battlepass);
		}

		private void OnGetPlayerSuccessful(object context, Player player)
		{
			CallbackHolder callbackHolder = (CallbackHolder)context;
			if (player != null)
			{
				this.SetPlayer(player);
			}
			if (callbackHolder.SucessCallBack != null)
			{
				callbackHolder.SucessCallBack();
			}
		}

		private void OnGetPlayerFailure(object context, Exception ex)
		{
			CallbackHolder callbackHolder = (CallbackHolder)context;
			UserInfo.Log.Fatal("Failed to get my player.", ex);
			if (callbackHolder.FailureCallBack != null)
			{
				callbackHolder.FailureCallBack();
			}
		}

		private IObservable<Character[]> RetrieveCharactersData()
		{
			return SwordfishObservable.FromSwordfishCall<Character[]>(delegate(SwordfishClientApi.ParameterizedCallback<Character[]> success, SwordfishClientApi.ErrorCallback error)
			{
				GameHubBehaviour.Hub.ClientApi.character.GetAllCharacters(this, this.PlayerSF.Id, success, error);
			});
		}

		public IObservable<Unit> UpdateCharactersDataStorage()
		{
			return Observable.AsUnitObservable<Character[]>(Observable.Do<Character[]>(this.RetrieveCharactersData(), delegate(Character[] charactersData)
			{
				this.Characters = charactersData;
			}));
		}

		public void ReloadCharactersData(object state, Action callback)
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
			{
				return;
			}
			GameHubBehaviour.Hub.ClientApi.character.GetAllCharacters(state, this.PlayerSF.Id, delegate(object x, Character[] characters)
			{
				this.OnCharactersDataRetrieveSuccess(x, characters, callback);
			}, delegate(object x, Exception exception)
			{
				this.OnCharactersDataRetrieveError(x, exception, callback);
			});
		}

		private void OnCharactersDataRetrieveSuccess(object state, Character[] characters, Action afterLoad)
		{
			this.Characters = characters;
			UserInfo.InvokeIfNotNull(afterLoad);
		}

		private void OnCharactersDataRetrieveError(object state, Exception exception, Action afterLoad)
		{
			UserInfo.Log.Fatal("Failed to get my characters.", exception);
			UserInfo.InvokeIfNotNull(afterLoad);
		}

		private static void InvokeIfNotNull(Action method)
		{
			if (method == null)
			{
				return;
			}
			method();
		}

		public CharacterBag GetCharacterBag(Guid itemTypeId)
		{
			for (int i = 0; i < this.Characters.Length; i++)
			{
				Character character = this.Characters[i];
				CharacterBag characterBag = (CharacterBag)((JsonSerializeable<!0>)character.Bag);
				if (characterBag.CharacterId == itemTypeId)
				{
					return characterBag;
				}
			}
			return null;
		}

		public void ConnectToServer(bool reconnect, Action onFailCallback, Action onSuccessCallback = null)
		{
			this.InternalConnectToServer(false, reconnect, onFailCallback, onSuccessCallback);
		}

		public void ConnectNarratorToServer(bool reconnect, Action onFailCallback, Action onSuccessCallback = null)
		{
			this.InternalConnectToServer(true, reconnect, onFailCallback, onSuccessCallback);
		}

		public void ConnectToHORTA(MatchData data)
		{
			this.IsNarrator = true;
			this.IsReconnecting = false;
			if (SingletonMonoBehaviour<SpectatorController>.DoesInstanceExist())
			{
				SingletonMonoBehaviour<SpectatorController>.Instance.CurrentSpectatorRole = SpectatorRole.Spectator;
			}
			if (this.OnMatchDataReceived != null)
			{
				this.OnMatchDataReceived(data.State);
			}
		}

		private void Awake()
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				this.IsClusterAuth = GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.DebugClusterAuth);
			}
		}

		private void InternalConnectToServer(bool narrator, bool reconnect, Action onFailCallback, Action onSuccessCallback = null)
		{
			if (this.HackIsWaitingForMatchServerConnection)
			{
				return;
			}
			this.IsNarrator = narrator;
			this.IsReconnecting = reconnect;
			GameHubBehaviour.Hub.Match.State = MatchData.MatchState.Nothing;
			NetworkClient networkClient = (NetworkClient)GameHubBehaviour.Hub.Net;
			NetworkReturn ret = networkClient.OpenConnection(GameHubBehaviour.Hub.Server.ServerIp, GameHubBehaviour.Hub.Server.ServerPort);
			UserInfo.Log.DebugFormat("{0}Connecting{1} to server={2}:{3}", new object[]
			{
				(!reconnect) ? string.Empty : "Re",
				(!narrator) ? string.Empty : " narrator",
				GameHubBehaviour.Hub.Server.ServerIp,
				GameHubBehaviour.Hub.Server.ServerPort
			});
			base.StartCoroutine(this.WaitForConnection(ret, onFailCallback, onSuccessCallback));
		}

		private IEnumerator WaitForConnection(NetworkReturn ret, Action onFailCallback, Action onSuccessCallback)
		{
			GameHubBehaviour.Hub.Server.ConnectStarted = true;
			this.HackIsWaitingForMatchServerConnection = true;
			if (ret != NetworkReturn.Ok)
			{
				UserInfo.Log.ErrorFormat("Connect Error={0}", new object[]
				{
					ret
				});
				GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ForceCloseWaitingWindow();
				this.HackIsWaitingForMatchServerConnection = false;
				this.ConnectionFailed(onFailCallback);
				yield break;
			}
			NetworkClient netClient = (NetworkClient)GameHubBehaviour.Hub.Net;
			UserInfo.Log.Info("Awaiting to establish connection.");
			while (netClient.State != SessionState.Established && netClient.State != SessionState.Closed)
			{
				yield return null;
			}
			if (netClient.State != SessionState.Closed)
			{
				Stopwatch watch = new Stopwatch();
				int timeout = 1000 * GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.SfNetClockSyncTimeout);
				UserInfo.Log.Debug("Connected, awaiting for clock sync.");
				watch.Start();
				while (!GameHubBehaviour.Hub.Clock.IsSynchronized && watch.ElapsedMilliseconds < (long)timeout)
				{
					yield return null;
				}
				watch.Stop();
			}
			if (!GameHubBehaviour.Hub.Clock.IsSynchronized)
			{
				netClient.CloseConnection();
				string msg = string.Format("SteamID={0}", GameHubBehaviour.Hub.ClientApi.hubClient.Id);
				GameHubBehaviour.Hub.Swordfish.Log.BILogClientMsg(46, msg, false);
			}
			if (netClient.State == SessionState.Closed)
			{
				UserInfo.Log.Info("Failed to connect.");
				this.HackIsWaitingForMatchServerConnection = false;
				if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
				{
					GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.Fail();
				}
				GameHubBehaviour.Hub.GuiScripts.Loading.HideLoading();
				GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ForceCloseWaitingWindow();
				this.ConnectionFailed(onFailCallback);
				yield break;
			}
			UserInfo.Log.Info("Clock synchronized, awaiting match data.");
			while (GameHubBehaviour.Hub.Match.State == MatchData.MatchState.Nothing)
			{
				yield return null;
			}
			yield return UserInfo.WaitForMatchPlayersData();
			UserInfo.Log.Info("Still waiting others' connections.");
			while (GameHubBehaviour.Hub.Match.State == MatchData.MatchState.AwaitingConnections)
			{
				yield return null;
			}
			if (onSuccessCallback != null)
			{
				onSuccessCallback();
			}
			if (SingletonMonoBehaviour<SpectatorController>.DoesInstanceExist())
			{
				if (this.IsNarrator)
				{
					SingletonMonoBehaviour<SpectatorController>.Instance.CurrentSpectatorRole = SpectatorRole.Spectator;
				}
				else
				{
					SingletonMonoBehaviour<SpectatorController>.Instance.CurrentSpectatorRole = SpectatorRole.None;
				}
			}
			GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ForceCloseWaitingWindow();
			Assembly SWF = GameHubBehaviour.Hub.ClientApi.GetType().Assembly;
			string swfVer = FileVersionInfo.GetVersionInfo(SWF.Location).FileVersion;
			GameHubBehaviour.Hub.VersionChecker.SendVersion(swfVer, "Release.15.00.250");
			UserInfo.Log.InfoFormat("All passed. Match State:{0}", new object[]
			{
				GameHubBehaviour.Hub.Match.State
			});
			switch (GameHubBehaviour.Hub.Match.State)
			{
			case MatchData.MatchState.Tutorial:
				GameHubBehaviour.Hub.State.GotoState(this.HackLoadingMode, false);
				break;
			case MatchData.MatchState.CharacterPick:
				if (false || this.IsReconnecting)
				{
					this.ReconnectGoToPick();
				}
				break;
			case MatchData.MatchState.PreMatch:
			case MatchData.MatchState.MatchStarted:
			case MatchData.MatchState.MatchOverRedWins:
			case MatchData.MatchState.MatchOverBluWins:
			case MatchData.MatchState.MatchOverTie:
				GameHubBehaviour.Hub.GuiScripts.Loading.ShowDefaultLoading(false);
				this.ReconnectGoToLoading();
				break;
			}
			UserInfo.Log.InfoFormat("Done, match data received={0}", new object[]
			{
				GameHubBehaviour.Hub.Match.State
			});
			this.HackIsWaitingForMatchServerConnection = false;
			if (this.OnMatchDataReceived != null)
			{
				this.OnMatchDataReceived(GameHubBehaviour.Hub.Match.State);
			}
			yield break;
		}

		private static IEnumerator WaitForMatchPlayersData()
		{
			UserInfo.Log.Info("Awaiting players data.");
			while (GameHubBehaviour.Hub.Players.CurrentPlayerData == null)
			{
				yield return null;
			}
			yield break;
		}

		private void ReconnectGoToPick()
		{
			DisposableExtensions.AddTo<IDisposable>(ObservableExtensions.Subscribe<Unit>(Observable.DoOnCompleted<Unit>(this.PrepareMatch(), delegate()
			{
				if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.EnableDrafter))
				{
					GameHubBehaviour.Hub.State.GotoState(this.CharacterSelectionState, false);
				}
				else
				{
					GameHubBehaviour.Hub.State.GotoState(this.HackPickMode, false);
				}
			})), this);
		}

		private void ReconnectGoToLoading()
		{
			DisposableExtensions.AddTo<IDisposable>(ObservableExtensions.Subscribe<Unit>(Observable.DoOnCompleted<Unit>(this.PrepareMatch(), delegate()
			{
				UserInfo.Log.Info("TOMATE GoingToLoading After PrepareMatch");
				GameHubBehaviour.Hub.State.GotoState(this.HackLoadingMode, false);
			})), this);
		}

		private IObservable<Unit> PrepareMatch()
		{
			return Observable.ContinueWith<Unit, Unit>(this.WaitUntilMatchPlayersAreReceived(), (Unit _) => this._diContainer.Resolve<IPrepareMatch>().Prepare());
		}

		private IObservable<Unit> WaitUntilMatchPlayersAreReceived()
		{
			return Observable.AsUnitObservable<long>(Observable.First<long>(Observable.EveryUpdate(), (long _) => GameHubBehaviour.Hub.Players.Clients.Count > 0));
		}

		private void ConnectionFailed(Action onFailCallback)
		{
			if (SingletonMonoBehaviour<SpectatorController>.DoesInstanceExist())
			{
				SingletonMonoBehaviour<SpectatorController>.Instance.CurrentSpectatorRole = SpectatorRole.None;
			}
			UserInfo.OkWindowFeedback(this.OnConnectionErrorDraft, TranslationContext.GUI, onFailCallback, new object[0]);
		}

		private static void OkWindowFeedback(string key, ContextTag tab, Action onOk, params object[] param)
		{
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.GetFormatted(key, tab, param),
				OkButtonText = Language.Get("Ok", TranslationContext.GUI),
				OnOk = delegate()
				{
					if (onOk != null)
					{
						onOk();
					}
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		[Inject]
		private IRemotelyEnabledFeaturesNamesStorage _remotelyEnabledFeaturesNamesStorage;

		[Inject]
		private IInitializeRemotelyToggleableFeatures _initializeRemotelyToggleableFeatures;

		private static readonly BitLogger Log = new BitLogger(typeof(UserInfo));

		[Inject]
		private DiContainer _diContainer;

		public PlayerInventory Inventory;

		public SwordfishStore Store;

		public string Name;

		public string OnConnectionErrorDraft = "FAILED_TO_CONNECT";

		private string _onMatchEndedDraft = "MatchFailedToConnectMatchEnded";

		public User UserSF = new User();

		public Player PlayerSF = new Player();

		private PlayerBag _bag = new PlayerBag();

		[SerializeField]
		private BattlepassProgressScriptableObject _battlepassProgress;

		private Wallet _wallet = new Wallet();

		public Character[] Characters;

		public bool IsReconnecting;

		public bool IsNarrator;

		public GameState HackPickMode;

		public GameState HackLoadingMode;

		public CharacterSelectionClientState CharacterSelectionState;

		public bool HackIsWaitingForMatchServerConnection;

		public delegate void UserUniversalIdTaken();

		public delegate void MatchDataReceived(MatchData.MatchState matchState);
	}
}
