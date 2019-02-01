using System;
using ClientAPI.Objects;
using HeavyMetalMachines.Swordfish;
using Pocketverse;
using Swordfish.Common.exceptions;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ReconnectMenu : GameState
	{
		protected override void OnMyLevelLoaded()
		{
			this._hmmHub = GameHubBehaviour.Hub;
			if (this._reconnectMenuGui == null)
			{
				this._reconnectMenuGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<ReconnectMenuGui>();
			}
			if (this._firstReconnect && !this._hmmHub.User.Bag.CurrentIsNarrator)
			{
				this._firstReconnect = false;
				this._reconnectMenuGui.ShowReconnectWindow();
				this.BackToGame();
				return;
			}
			this._reconnectMenuGui.ShowButtons();
		}

		public void BackToMain()
		{
			this.ClearCurrentServer();
		}

		public void BackToGame()
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
			{
				this.TryToReconnect();
				return;
			}
			this._hmmHub.User.ReloadPlayer(new Action(this.TryToReconnect), new Action(this.OnReconnectFail));
		}

		private void ClearCurrentServer()
		{
			PlayerCustomWS.ClearCurrentServer(delegate(object x, string res)
			{
				this.GoToMainMenu();
			}, delegate(object x, Exception ex)
			{
				ReconnectMenu.Log.Fatal("Error clearing server from bag.", ex);
				this._reconnectMenuGui.ShowButtons();
			});
		}

		private void GoToMainMenu()
		{
			GameHubBehaviour.Hub.State.GotoState(GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.MainMenu), false);
		}

		private void OnReconnectFail()
		{
			this._reconnectMenuGui.ShowButtons();
		}

		private void OnReconnectSuccess()
		{
			this._reconnectMenuGui.HideConfirmWindow();
		}

		private void TryToReconnect()
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
			{
				this._hmmHub.Characters.SetRotationCharacters(null);
				this._hmmHub.User.ConnectToServer(true, new Action(this.OnReconnectFail), new Action(this.OnReconnectSuccess));
				return;
			}
			Guid matchIdGUID;
			if (string.IsNullOrEmpty(this._hmmHub.User.Bag.CurrentMatchId))
			{
				matchIdGUID = Guid.Empty;
			}
			else
			{
				matchIdGUID = new Guid(this._hmmHub.User.Bag.CurrentMatchId);
			}
			string regionName = this._hmmHub.Config.GetSetting("CURRENT_REGION", this._hmmHub.ClientApi.GetCurrentRegionName());
			if (!SingletonMonoBehaviour<RegionController>.Instance.RegionDictionary.ContainsKey(regionName))
			{
				regionName = this._hmmHub.ClientApi.GetCurrentRegionName();
				ReconnectMenu.Log.WarnFormat("Region not available, falling back to {0}", new object[]
				{
					regionName
				});
			}
			this._hmmHub.ClientApi.cluster.GetGameServerRunningByMatchId(null, regionName, matchIdGUID, delegate(object state, GameServerInfo info)
			{
				if (info == null)
				{
					ReconnectMenu.Log.Warn("Match ended while player was on reconnect, no longer valid");
					this._reconnectMenuGui.WarnMatchAlreadyEnded();
					return;
				}
				this.ReconnectToGame();
			}, delegate(object state, Exception exception)
			{
				if (exception is RegionNotFoundException)
				{
					ReconnectMenu.Log.Warn("Region not found, MatchId is not valid");
					this._reconnectMenuGui.WarnMatchAlreadyEnded();
					return;
				}
				ReconnectMenu.Log.ErrorFormat("Exception on GetGameServerRunningByMatchId. matchId: {0},region:{1}, Exception: {2}", new object[]
				{
					matchIdGUID,
					regionName,
					exception
				});
				this.OnReconnectFail();
			});
		}

		private void ReconnectToGame()
		{
			MainMenuCustomWS.GetMainMenuData(this, delegate(object state, string strMainMenuData)
			{
				MainMenuData mainMenuData = (MainMenuData)((JsonSerializeable<T>)strMainMenuData);
				this._hmmHub.Characters.SetRotationCharacters(mainMenuData.CurrentRotation);
				this._hmmHub.User.SetPlayer(mainMenuData.Player);
				this._hmmHub.User.SetBattlepassProgress(mainMenuData.BattlepassProgressString);
				this._hmmHub.User.Inventory.SetAllReloadedItems(mainMenuData.PlayerInventories, mainMenuData.PlayerItems);
				this._hmmHub.GuiScripts.TopMenu.UpdateCurrencyLabels();
				this._hmmHub.GuiScripts.TopMenu.RefreshUserXp();
				if (this._hmmHub.User.Bag.CurrentMatchId == null || this._hmmHub.User.Bag.CurrentServerIp == null || this._hmmHub.User.Bag.CurrentPort == 0)
				{
					ReconnectMenu.Log.Warn("Match ended while getting MainMenuData, no longer valid");
					this._reconnectMenuGui.WarnMatchAlreadyEnded();
					return;
				}
				this._hmmHub.Server.ServerIp = this._hmmHub.User.Bag.CurrentServerIp;
				this._hmmHub.Server.ServerPort = this._hmmHub.User.Bag.CurrentPort;
				this._hmmHub.Swordfish.Msg.ClientMatchId = new Guid(this._hmmHub.User.Bag.CurrentMatchId);
				if (this._hmmHub.User.Bag.CurrentIsNarrator)
				{
					this._hmmHub.User.ConnectNarratorToServer(true, new Action(this.OnReconnectFail), new Action(this.OnReconnectSuccess));
				}
				else
				{
					this._hmmHub.User.ConnectToServer(true, new Action(this.OnReconnectFail), new Action(this.OnReconnectSuccess));
				}
			}, delegate(object x, Exception e)
			{
				ReconnectMenu.Log.Error("Failed to get MainMenuData on reconnect", e);
				Debug.LogError("Failed to get MainMenuData on reconnect - error: " + e);
				this.OnReconnectFail();
			});
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ReconnectMenu));

		private HMMHub _hmmHub;

		private bool _firstReconnect = true;

		private ReconnectMenuGui _reconnectMenuGui;
	}
}
