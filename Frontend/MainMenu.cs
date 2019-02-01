using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI;
using ClientAPI.MessageHub;
using ClientAPI.Objects;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Tutorial;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenu : GameState, ISwordfishWebServiceTimeOut
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event System.Action PlayerReloadedEvent;

		private void OnPlayerReloaded()
		{
			if (MainMenu.PlayerReloadedEvent != null)
			{
				MainMenu.PlayerReloadedEvent();
			}
		}

		protected override void OnStateEnabled()
		{
			this._hub = GameHubBehaviour.Hub;
			this._isMainMenuEnable = true;
			this._skipSwordfish = GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false);
			if (this._skipSwordfish)
			{
				return;
			}
			this._hub.Swordfish.Log.BILogClient(ClientBITags.MainMenuStart, true);
			if (!this._callbacksInstalled)
			{
				this._hub.Swordfish.Msg.MsgHub.Disconnected += this.OnMsgHubDisconnected;
				this._callbacksInstalled = true;
			}
			using (StreamWriter streamWriter = new StreamWriter("cuf"))
			{
				streamWriter.Write(GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.CrashPageURL), Language.CurrentLanguage(), this._hub.User.UserSF.UniversalID);
			}
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.ShowWelcomePage) && !TutorialController.HasPlayerDoneFirstTutorial())
			{
				this.ShowWelcomePage();
			}
		}

		private void ShowWelcomePage()
		{
			string value = GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.WelcomePageURL);
			if (0 < value.Length)
			{
				try
				{
					string text = string.Format(value, Language.CurrentLanguage(), this._hub.User.UserSF.UniversalID);
					this._hub.Swordfish.Log.BILogClientMsg(ClientBITags.RequestWelcomePage, text, true);
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
			this._hub.Swordfish.Log.BILogClient(ClientBITags.MainMenuEnd, true);
			this._isMainMenuEnable = false;
			this._hub.Swordfish.Connection.DeregisterConnectionMonitoring();
			if (this.MainMenuGui != null)
			{
				this.MainMenuGui = null;
			}
			if (this._callbacksInstalled)
			{
				this._hub.Swordfish.Msg.MsgHub.Disconnected -= this.OnMsgHubDisconnected;
				this._callbacksInstalled = false;
			}
		}

		protected override void OnMyLevelLoaded()
		{
			this.MainMenuGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>();
			this.GetMainMenuData();
			this._hub.Swordfish.Connection.RegisterConnectionMonitoring();
			this._hub.User.ReloadCharacters(this, null);
			ManagerController.Get<FriendBagManager>().SaveMyFriendBagOnNextFrame();
		}

		public void GetMainMenuData()
		{
			if (this._requestedMainMenuData)
			{
				return;
			}
			this._requestedMainMenuData = true;
			if (this._skipSwordfish)
			{
				this.GetMainMenuDataSkipSwordfish();
				return;
			}
			MainMenuCustomWS.GetMainMenuData(this, new SwordfishClientApi.ParameterizedCallback<string>(this.OnGetMainMenuDataSuccess), new SwordfishClientApi.ErrorCallback(this.OnGetMainMenuDataFailure));
		}

		private void GetMainMenuDataSkipSwordfish()
		{
			this._requestedMainMenuData = false;
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.NoRotation))
			{
				this._hub.Characters.SetRotationCharacters(null);
				return;
			}
			Week debugRotationWeek = this._hub.Characters.GetDebugRotationWeek();
			this._hub.Characters.SetRotationCharacters(debugRotationWeek);
		}

		private void OnGetMainMenuDataFailure(object state, Exception e)
		{
			this._requestedMainMenuData = false;
			MainMenu.Log.Error("Failed to get MainMenuData", e);
			UnityEngine.Debug.LogError("Failed to get MainMenuData - error: " + e);
		}

		private void OnGetMainMenuDataSuccess(object state, string strMainMenuData)
		{
			this._requestedMainMenuData = false;
			MainMenu.Log.WarnFormat("Received MainMenuData", new object[0]);
			if (!this._isMainMenuEnable)
			{
				MainMenu.Log.WarnFormat("Player is no longer in MainMenu", new object[0]);
				return;
			}
			this.MainMenuGui.ConnectionFeedback.ChangeVisibilityConnectionFeedback(false);
			MainMenuData mainMenuData = (MainMenuData)((JsonSerializeable<T>)strMainMenuData);
			this._hub.Characters.SetRotationCharacters(mainMenuData.CurrentRotation);
			this._hub.User.SetPlayer(mainMenuData.Player);
			this._hub.User.SetBattlepassProgress(mainMenuData.BattlepassProgressString);
			this._hub.Store.SetBalance(mainMenuData.SoftCurrency, mainMenuData.HardCurrency);
			this._hub.Store.SetItemPrices(mainMenuData.AllStoreItemPrices);
			this._hub.User.Inventory.SetAllReloadedItems(mainMenuData.PlayerInventories, mainMenuData.PlayerItems);
			this._hub.User.Inventory.HasNewItems = (mainMenuData.NewItemsCount > 0);
			this._hub.GuiScripts.TopMenu.UpdateCurrencyLabels();
			this._hub.GuiScripts.TopMenu.RefreshUserXp();
			this.OnPlayerReloaded();
			this.SyncServerTime();
			this._hub.GuiScripts.TopMenu.RefreshPlayerCustomizations();
			this._hub.GuiScripts.TopMenu.RefreshGroupCustomizations();
			this.MainMenuGui.OnGetMainMenuDataSuccess(mainMenuData);
			MainMenu.Log.WarnFormat("Finished Received MainMenuData", new object[0]);
		}

		public void SyncServerTime()
		{
			StoreCustomWS.SyncServerTime(delegate(object state, string s)
			{
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
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = guid,
				QuestionText = Language.Get("LostMessageHubConnection", "GUI"),
				OkButtonText = Language.Get("Ok", "GUI"),
				OnOk = delegate()
				{
					try
					{
						if (!this._skipSwordfish)
						{
							GameHubBehaviour.Hub.Swordfish.Msg.Cleanup();
						}
					}
					catch (Exception ex)
					{
					}
					GameHubBehaviour.Hub.Quit();
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
			((NetworkClient)GameHubBehaviour.Hub.Net).CloseConnection();
			MainMenu.Log.Error("MsgHub disconnected, closing Application.");
			Mural.PostAll(default(CleanupMessage), typeof(ICleanupListener));
		}

		public bool CloseGame()
		{
			return false;
		}

		public string TimeOutMessage()
		{
			return Language.Get("FAILED_TO_CONNECT", "GUI");
		}

		public void TimeOut()
		{
			MainMenu.Log.Warn("Main Menu Data TimeOut");
			if (this._isMainMenuEnable)
			{
				this.MainMenuGui.ConnectionFeedback.ChangeVisibilityConnectionFeedback(true);
				return;
			}
		}

		public bool SearchForAMatch(string queueName, System.Action onError)
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish) || GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipMatchmaking))
			{
				if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.EnableNarrator))
				{
					GameHubBehaviour.Hub.Server.ServerPort = GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.ServerPort);
					GameHubBehaviour.Hub.Server.ServerIp = GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.ServerIP);
					GameHubBehaviour.Hub.User.ConnectNarratorToServer(false, delegate
					{
						GameHubBehaviour.Hub.State.GotoState(this, false);
					}, null);
				}
				else
				{
					GameHubBehaviour.Hub.User.ConnectToServer(false, delegate
					{
						GameHubBehaviour.Hub.State.GotoState(this, false);
					}, null);
				}
			}
			else
			{
				SwordfishMatchmaking matchmaking = this._hub.Swordfish.Msg.Matchmaking;
				if (this._hub.User.PartyId == Guid.Empty)
				{
					matchmaking.StartMatch(queueName, onError);
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
					matchmaking.StartGroupMatch(this._hub.User.PartyId, list.ToArray(), queueName, onError);
				}
			}
			return false;
		}

		public void CancelMatchMaking()
		{
			this._hub.Swordfish.Msg.Matchmaking.StopSearch();
		}

		public void GoToMatch()
		{
			this._hub.Swordfish.Msg.ConnectToMatch(this, null);
		}

		public void SendMatchAccepted()
		{
			this._hub.Swordfish.Msg.Matchmaking.Accept();
		}

		public void SendRejectMatch()
		{
			this._hub.Swordfish.Msg.Matchmaking.Decline();
		}

		public void ClearCurrentServer()
		{
			PlayerCustomWS.ClearCurrentServer(delegate(object x, string res)
			{
			}, delegate(object x, Exception ex)
			{
				MainMenu.Log.Fatal("Error clearing server from bag.", ex);
			});
		}

		public void Quit()
		{
			try
			{
				if (!this._skipSwordfish)
				{
					this._hub.Swordfish.Msg.Cleanup();
				}
			}
			catch (Exception ex)
			{
			}
			GameHubBehaviour.Hub.Quit();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(MainMenu));

		private HMMHub _hub;

		private MainMenuGui MainMenuGui;

		public Profile ProfileState;

		public Item ItemState;

		public CharacterWorkshop CharacterWorkshopState;

		public Options OptionsState;

		private bool _skipSwordfish;

		private bool _callbacksInstalled;

		private bool _isMainMenuEnable;

		private bool _requestedMainMenuData;
	}
}
