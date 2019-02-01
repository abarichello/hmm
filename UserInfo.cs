using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI;
using ClientAPI.Objects;
using Commons.Swordfish.Battlepass;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.ScriptableObjects;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;

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

		public void GetLoginData(System.Action onPlayerOk, System.Action onGetError, System.Action onTimeOut)
		{
			UserInfo.CallbackHolder state = new UserInfo.CallbackHolder
			{
				SucessCallBack = onPlayerOk,
				FailureCallBack = onGetError,
				TimeOutCallBack = onTimeOut
			};
			LoginCustomWS.GetLoginData(state, new SwordfishClientApi.ParameterizedCallback<string>(this.OnGetLoginDataSucessful), new SwordfishClientApi.ErrorCallback(this.OnGetLoginDataFailure));
		}

		private void OnGetLoginDataSucessful(object context, string data)
		{
			UserInfo.CallbackHolder callbackHolder = (UserInfo.CallbackHolder)context;
			NetResult netResult = (NetResult)((JsonSerializeable<T>)data);
			if (!netResult.Success)
			{
				UserInfo.Log.Error("Error onGetLoginData", new Exception(netResult.Msg));
				callbackHolder.FailureCallBack();
				return;
			}
			LoginData loginData = (LoginData)((JsonSerializeable<T>)netResult.Msg);
			this.UserSF = loginData.MyUser;
			this.UniversalId = loginData.MyUser.UniversalID;
			this.SetPlayer(loginData.MyPlayer);
			this.Characters = loginData.MyCharacters;
			this._wallet = loginData.MyWallet;
			this.Name = loginData.MyUser.Login;
			UserInfo.Log.InfoFormat("{0}-Player={1} UserId={2} UniversalId={3} Bag={4}", new object[]
			{
				this.PlayerSF.Id,
				this.PlayerSF.Name,
				this.UserSF.Id,
				this.UniversalId,
				this.PlayerSF.Bag
			});
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
			GameHubBehaviour.Hub.PlayerPrefs.Load();
			callbackHolder.SucessCallBack();
		}

		private void OnGetLoginDataFailure(object context, Exception ex)
		{
			UserInfo.CallbackHolder callbackHolder = (UserInfo.CallbackHolder)context;
			if (callbackHolder.FailureCallBack != null)
			{
				UserInfo.Log.Error("Error onGetLoginData", ex);
				callbackHolder.FailureCallBack();
			}
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
			this._battlepassProgress.UpdateBattlepassProgress((BattlepassProgress)((JsonSerializeable<T>)battlepassProgressBag));
		}

		public void SetTestBattlepassProgressScriptableObject(BattlepassProgressScriptableObject progress)
		{
			this._battlepassProgress = progress;
		}

		public void ReloadPlayer(System.Action afterLoad, System.Action failAction)
		{
			if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
			{
				UserInfo.CallbackHolder state = new UserInfo.CallbackHolder
				{
					SucessCallBack = afterLoad,
					FailureCallBack = failAction
				};
				GameHubBehaviour.Hub.ClientApi.user.GetMyPlayer(state, new SwordfishClientApi.ParameterizedCallback<Player>(this.OnGetPlayerSuccessful), new SwordfishClientApi.ErrorCallback(this.OnGetPlayerFailure));
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
			UserInfo.CallbackHolder callbackHolder = (UserInfo.CallbackHolder)context;
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
			UserInfo.CallbackHolder callbackHolder = (UserInfo.CallbackHolder)context;
			UserInfo.Log.Fatal("Failed to get my player.", ex);
			if (callbackHolder.FailureCallBack != null)
			{
				callbackHolder.FailureCallBack();
			}
		}

		public void ReloadCharacters(object state, System.Action afterLoad)
		{
			if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
			{
				GameHubBehaviour.Hub.ClientApi.character.GetAllCharacters(state, this.PlayerSF.Id, delegate(object x, Character[] y)
				{
					this.Characters = y;
					if (afterLoad != null)
					{
						afterLoad();
					}
				}, delegate(object x, Exception y)
				{
					UserInfo.Log.Fatal("Failed to get my characters.", y);
					if (afterLoad != null)
					{
						afterLoad();
					}
				});
			}
		}

		public CharacterBag GetCurrentCharacterBag()
		{
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData == null || GameHubBehaviour.Hub.Players.CurrentPlayerData.Character == null)
			{
				return null;
			}
			for (int i = 0; i < this.Characters.Length; i++)
			{
				Character character = this.Characters[i];
				CharacterBag characterBag = (CharacterBag)((JsonSerializeable<T>)character.Bag);
				if (characterBag.CharacterId == GameHubBehaviour.Hub.Players.CurrentPlayerData.Character.CharacterItemTypeGuid)
				{
					return characterBag;
				}
			}
			return null;
		}

		public CharacterBag GetCharacterBag(CharacterHierarchy characterHierarchy)
		{
			return this.GetCharacterBag(characterHierarchy.CharacterItemType.Id);
		}

		public CharacterBag GetCharacterBag(Guid itemTypeId)
		{
			for (int i = 0; i < this.Characters.Length; i++)
			{
				Character character = this.Characters[i];
				CharacterBag characterBag = (CharacterBag)((JsonSerializeable<T>)character.Bag);
				if (characterBag.CharacterId == itemTypeId)
				{
					return characterBag;
				}
			}
			return null;
		}

		public CharacterBag SetCharacterBagItemUnlockSeen(Guid itemTypeId, int unlockLevel)
		{
			for (int i = 0; i < this.Characters.Length; i++)
			{
				Character character = this.Characters[i];
				CharacterBag characterBag = (CharacterBag)((JsonSerializeable<T>)character.Bag);
				if (characterBag.CharacterId == itemTypeId)
				{
					characterBag.SetUnlockSeen(unlockLevel);
					character.Bag = (string)characterBag;
					return characterBag;
				}
			}
			return null;
		}

		public void ConnectToServer(bool reconnect, System.Action onFailCallback, System.Action onSuccessCallback = null)
		{
			this.InternalConnectToServer(false, reconnect, onFailCallback, onSuccessCallback);
		}

		public void ConnectNarratorToServer(bool reconnect, System.Action onFailCallback, System.Action onSuccessCallback = null)
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

		private void InternalConnectToServer(bool narrator, bool reconnect, System.Action onFailCallback, System.Action onSuccessCallback = null)
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
			base.StartCoroutine(this.WaitForConnection(ret, onFailCallback, onSuccessCallback));
		}

		private IEnumerator WaitForConnection(NetworkReturn ret, System.Action onFailCallback, System.Action onSuccessCallback)
		{
			GameHubBehaviour.Hub.Server.ConnectStarted = true;
			this.HackIsWaitingForMatchServerConnection = true;
			if (ret != NetworkReturn.Ok)
			{
				GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ForceCloseWaitingWindown();
				this.HackIsWaitingForMatchServerConnection = false;
				this.ConnectionFailed(onFailCallback);
				yield break;
			}
			NetworkClient netClient = (NetworkClient)GameHubBehaviour.Hub.Net;
			while (netClient.State != SessionState.Established && netClient.State != SessionState.Closed)
			{
				yield return null;
			}
			if (netClient.State != SessionState.Closed)
			{
				Stopwatch watch = new Stopwatch();
				int timeout = 1000 * GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.SfNetClockSyncTimeout);
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
				GameHubBehaviour.Hub.Swordfish.Log.BILogClientMsg(ClientBITags.ClockSyncDisconnect, msg, false);
			}
			if (netClient.State == SessionState.Closed)
			{
				this.HackIsWaitingForMatchServerConnection = false;
				if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
				{
					GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.Fail();
				}
				GameHubBehaviour.Hub.GuiScripts.Loading.HideLoading();
				GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ForceCloseWaitingWindown();
				this.ConnectionFailed(onFailCallback);
				yield break;
			}
			while (GameHubBehaviour.Hub.Match.State == MatchData.MatchState.Nothing)
			{
				yield return null;
			}
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
			GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ForceCloseWaitingWindown();
			Assembly SWF = GameHubBehaviour.Hub.ClientApi.GetType().Assembly;
			string swfVer = FileVersionInfo.GetVersionInfo(SWF.Location).FileVersion;
			GameHubBehaviour.Hub.VersionChecker.SendVersion(swfVer, "2.07.972");
			switch (GameHubBehaviour.Hub.Match.State)
			{
			case MatchData.MatchState.Tutorial:
				GameHubBehaviour.Hub.State.GotoState(this.HackLoadingMode, false);
				break;
			case MatchData.MatchState.CharacterPick:
				if (false || this.IsReconnecting)
				{
					GameHubBehaviour.Hub.State.GotoState(this.HackPickMode, false);
				}
				break;
			case MatchData.MatchState.PreMatch:
			case MatchData.MatchState.MatchStarted:
			case MatchData.MatchState.MatchOverRedWins:
			case MatchData.MatchState.MatchOverBluWins:
			case MatchData.MatchState.MatchOverTie:
				GameHubBehaviour.Hub.GuiScripts.Loading.ShowDefaultLoading(false);
				GameHubBehaviour.Hub.State.GotoState(this.HackLoadingMode, false);
				break;
			}
			this.HackIsWaitingForMatchServerConnection = false;
			if (this.OnMatchDataReceived != null)
			{
				this.OnMatchDataReceived(GameHubBehaviour.Hub.Match.State);
			}
			yield break;
		}

		private void ConnectionFailed(System.Action onFailCallback)
		{
			if (SingletonMonoBehaviour<SpectatorController>.DoesInstanceExist())
			{
				SingletonMonoBehaviour<SpectatorController>.Instance.CurrentSpectatorRole = SpectatorRole.None;
			}
			UserInfo.OkWindowFeedback(this.OnConnectionErrorDraft, TranslationSheets.GUI, onFailCallback, new object[0]);
		}

		public static void OkWindowFeedback(string key, TranslationSheets tab, System.Action onOk, params object[] param)
		{
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = string.Format(Language.Get(key, tab), param),
				OkButtonText = Language.Get("Ok", TranslationSheets.GUI),
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

		private static readonly BitLogger Log = new BitLogger(typeof(UserInfo));

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

		private Wallet _wallet;

		public Character[] Characters;

		public bool IsReconnecting;

		public bool IsNarrator;

		public GameState HackPickMode;

		public GameState HackLoadingMode;

		public bool HackIsWaitingForMatchServerConnection;

		public delegate void UserUniversalIdTaken();

		public delegate void MatchDataReceived(MatchData.MatchState matchState);

		private class CallbackHolder : ISwordfishWebServiceTimeOut
		{
			public bool CloseGame()
			{
				return false;
			}

			public string TimeOutMessage()
			{
				return "Failed to Get MyPlayer Will Close the Game";
			}

			public void TimeOut()
			{
				Guid confirmWindowGuid = Guid.NewGuid();
				ConfirmWindowProperties properties = new ConfirmWindowProperties
				{
					Guid = confirmWindowGuid,
					QuestionText = string.Format(Language.Get("LostMessageHubConnection", TranslationSheets.GUI), new object[0]),
					OkButtonText = Language.Get("Ok", "GUI"),
					OnOk = delegate()
					{
						GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
						GameHubBehaviour.Hub.Quit();
					}
				};
				GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
				if (this.TimeOutCallBack != null)
				{
					this.TimeOutCallBack();
				}
			}

			public System.Action SucessCallBack;

			public System.Action FailureCallBack;

			public System.Action TimeOutCallBack;
		}
	}
}
