using System;
using ClientAPI;
using ClientAPI.Objects;
using HeavyMetalMachines.Achievements;
using HeavyMetalMachines.CharacterSelection.Rotation;
using HeavyMetalMachines.CompetitiveMode;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.LegacyStorage;
using HeavyMetalMachines.Social.Friends.Business.BlockedPlayers;
using HeavyMetalMachines.Social.Profile.Business;
using HeavyMetalMachines.Swordfish;
using Hoplon.Serialization;
using Hoplon.ToggleableFeatures;
using Pocketverse;
using Swordfish.Common.exceptions;
using UniRx;
using UnityEngine;
using Zenject;

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
				ReconnectMenu.Log.Debug("first reconnect");
				this._reconnectMenuGui.ShowReconnectWindow();
				this.BackToGame();
				return;
			}
			ReconnectMenu.Log.Debug("show reconnect buttons");
			this._reconnectMenuGui.ShowReconnectDialog();
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
				ReconnectMenu.Log.DebugFormat("Server cleared, result={0}", new object[]
				{
					res
				});
				this.GoToNextState();
			}, delegate(object x, Exception ex)
			{
				ReconnectMenu.Log.Fatal("Error clearing server from bag.", ex);
				this._reconnectMenuGui.ShowReconnectDialog();
			});
		}

		private void GoToNextState()
		{
			if (this._shouldCreateProfile.Get())
			{
				base.GoToState(GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.Profile), false);
				return;
			}
			GameHubBehaviour.Hub.State.GotoState(GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.MainMenu), false);
		}

		private void OnReconnectFail()
		{
			ReconnectMenu.Log.Debug("OnReconnectFail. show reconnect buttons");
			this._reconnectMenuGui.ShowReconnectDialog();
		}

		private void OnReconnectSuccess()
		{
			this._reconnectMenuGui.HideConfirmWindow();
		}

		private void TryToReconnect()
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
			{
				this._hmmHub.User.ConnectToServer(true, new Action(this.OnReconnectFail), new Action(this.OnReconnectSuccess));
				return;
			}
			Guid matchIdGUID;
			if (string.IsNullOrEmpty(this._hmmHub.User.Bag.CurrentMatchId))
			{
				ReconnectMenu.Log.Debug("CurrentMatchId null. Will using empty Guid");
				matchIdGUID = Guid.Empty;
			}
			else
			{
				matchIdGUID = new Guid(this._hmmHub.User.Bag.CurrentMatchId);
			}
			string regionName = this._hmmHub.PlayerPrefs.GetString("CURRENT_REGION", this._hmmHub.ClientApi.GetCurrentRegionName());
			ReconnectMenu.Log.DebugFormat("Asking swordfish if server is still alive. matchId: {0} region: {1}", new object[]
			{
				matchIdGUID,
				regionName
			});
			if (!SingletonMonoBehaviour<RegionController>.Instance.RegionDictionary.ContainsKey(regionName))
			{
				regionName = this._hmmHub.ClientApi.GetCurrentRegionName();
				ReconnectMenu.Log.WarnFormat("Region not available, falling back to {0}", new object[]
				{
					regionName
				});
			}
			this._hmmHub.ClientApi.cluster.GetGameServerRunningByMatchId(null, regionName, matchIdGUID, delegate(object state, GameServerRunningInfo info)
			{
				if (info == null)
				{
					ReconnectMenu.Log.Warn("Match ended while player was on reconnect, no longer valid");
					this._reconnectMenuGui.WarnMatchAlreadyEnded();
					return;
				}
				ReconnectMenu.Log.Debug("GameServer still alive. going back to game");
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
			MainMenuCustomWS.GetMainMenuData(this, new SwordfishClientApi.ParameterizedCallback<string>(this.OnMainMenuDataSuccess), delegate(object x, Exception e)
			{
				ReconnectMenu.Log.Error("Failed to get MainMenuData on reconnect", e);
				Debug.LogError("Failed to get MainMenuData on reconnect - error: " + e);
				this.OnReconnectFail();
			});
		}

		private void OnMainMenuDataSuccess(object state, string strMainMenuData)
		{
			ReconnectMenu.Log.DebugFormat("Received MainMenuData on reconnect", new object[0]);
			MainMenuData mainMenuData = (MainMenuData)((JsonSerializeable<!0>)strMainMenuData);
			this._mainMenuDataStorage.LatestMainMenuData = mainMenuData;
			this._rotationWeekStorage.Set(mainMenuData.CurrentRotationWeek);
			this._hmmHub.User.SetPlayer(mainMenuData.Player);
			this._hmmHub.User.SetBattlepassProgress(mainMenuData.BattlepassProgressString);
			this._hmmHub.User.Inventory.SetAllReloadedItems(mainMenuData.PlayerInventories, mainMenuData.PlayerItems);
			this._hmmHub.User.Inventory.RefreshPlayerCustomizations();
			ReconnectMenu.Log.DebugFormat("Finished Received MainMenuData on reconnect", new object[0]);
			if (this._hmmHub.User.Bag.CurrentMatchId == null || this._hmmHub.User.Bag.CurrentServerIp == null || this._hmmHub.User.Bag.CurrentPort == 0)
			{
				ReconnectMenu.Log.Warn("Match ended while getting MainMenuData, no longer valid");
				this._reconnectMenuGui.WarnMatchAlreadyEnded();
				return;
			}
			this._hmmHub.Server.ServerIp = this._hmmHub.User.Bag.CurrentServerIp;
			this._hmmHub.Server.ServerPort = this._hmmHub.User.Bag.CurrentPort;
			this._hmmHub.Swordfish.Msg.ClientMatchId = new Guid(this._hmmHub.User.Bag.CurrentMatchId);
			ObservableExtensions.Subscribe<Unit>(Observable.DoOnCompleted<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(this.InitializeToggleableFeatures(), (Unit _) => this.InitializeCompetitiveMode()), this._diContainer.Resolve<IBlockedPlayersInitializer>().Initialize()), this._fetchAchievements.Fetch()), new Action(this.ConnectToServer)));
		}

		private void ConnectToServer()
		{
			if (this._hmmHub.User.Bag.CurrentIsNarrator)
			{
				this._hmmHub.User.ConnectNarratorToServer(true, new Action(this.OnReconnectFail), new Action(this.OnReconnectSuccess));
				return;
			}
			this._hmmHub.User.ConnectToServer(true, new Action(this.OnReconnectFail), new Action(this.OnReconnectSuccess));
		}

		private IObservable<Unit> InitializeToggleableFeatures()
		{
			return this._initializeToggleableFeatures.Initialize();
		}

		private IObservable<Unit> InitializeCompetitiveMode()
		{
			return this._diContainer.Resolve<IInitializeCompetitiveMode>().Initialize();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ReconnectMenu));

		[InjectOnClient]
		private IInitializeToggleableFeatures _initializeToggleableFeatures;

		[InjectOnClient]
		private IRotationWeekStorage _rotationWeekStorage;

		[InjectOnClient]
		private DiContainer _diContainer;

		[InjectOnClient]
		private IShouldCreateProfile _shouldCreateProfile;

		[InjectOnClient]
		private MainMenuDataStorage _mainMenuDataStorage;

		[InjectOnClient]
		private IFetchAchievements _fetchAchievements;

		private HMMHub _hmmHub;

		private bool _firstReconnect = true;

		private ReconnectMenuGui _reconnectMenuGui;
	}
}
