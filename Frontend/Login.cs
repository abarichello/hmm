using System;
using System.Collections.Generic;
using ClientAPI;
using ClientAPI.Objects;
using Commons.Swordfish.Progression;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class Login
	{
		public Login(HMMHub hub, Action<bool> loginEndedCallback, Action<ConfirmWindowProperties> confirmWindowCallback)
		{
			this._hmmHub = hub;
			this._loginEndedCallback = loginEndedCallback;
			this._confirmWindowCallback = confirmWindowCallback;
			if (!this._hmmHub.Config.GetBoolValue(ConfigAccess.SkipSwordfish) && this._hmmHub.Swordfish.Msg.MsgHub != null)
			{
				this._hmmHub.Swordfish.Msg.MsgHub.Connected += this.OnMsgHubConnected;
				this._hmmHub.Swordfish.Msg.MsgHub.ConnectionError += this.OnMsgHubConnectError;
			}
		}

		public void ConnectSteam()
		{
			if (this._hmmHub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				this._hmmHub.User.Name = this._hmmHub.Config.GetValue(ConfigAccess.PlayerName);
				this._hmmHub.User.Bag = new PlayerBag
				{
					NickNameset = true,
					Level = 99
				};
				BattlepassProgress battlepassProgress = new BattlepassProgress();
				battlepassProgress.MissionProgresses = new List<MissionProgress>();
				battlepassProgress.FreeLevelsClaimed = new bool[0];
				battlepassProgress.PremiumLevelsClaimed = new bool[0];
				battlepassProgress.MissionsCompleted = new bool[0];
				this._hmmHub.User.SetBattlepassProgress(battlepassProgress.ToString());
				this._hmmHub.User.PlayerSF = new Player
				{
					Name = this._hmmHub.User.Name,
					Bag = this._hmmHub.User.Bag.ToString(),
					Id = (long)this._hmmHub.User.Name.GetHashCode()
				};
				this._hmmHub.PlayerPrefs.SkipSwordfishLoad();
				this._loginEndedCallback(true);
				return;
			}
			try
			{
				this._hmmHub.Swordfish.Log.BILogFunnel(FunnelBITags.SplashLoginStart, null);
				this._hmmHub.UserService.SteamLogin(null, new SwordfishClientApi.ParameterizedCallback<SteamLoginInfo>(this.SteamLoginSucess), new SwordfishClientApi.ErrorCallback(this.SteamLoginFail));
			}
			catch (Exception ex)
			{
				Login.Log.ErrorFormat("Steamworks failed. Will continue as normal sf. {0}", new object[]
				{
					ex
				});
				this._loginEndedCallback(false);
			}
		}

		private void SteamLoginSucess(object state, SteamLoginInfo loginInfo)
		{
			this._hmmHub.Swordfish.Connection.SessionId = loginInfo.SesssionId;
			bool flag = true;
			for (int i = 0; i < loginInfo.EnvironmentConfigurations.Length; i++)
			{
				EnvironmentConfiguration environmentConfiguration = loginInfo.EnvironmentConfigurations[i];
				if (environmentConfiguration.Id == 2 && environmentConfiguration.Description == "Expected Client Version")
				{
					flag = (environmentConfiguration.Value == "2.07.972");
					break;
				}
			}
			if (flag)
			{
				this._hmmHub.Swordfish.Log.BILogFunnel(FunnelBITags.SplashLoginSuccess, null);
				this.GetSwordfishSessionId();
			}
			else
			{
				this._hmmHub.Swordfish.Log.BILogClientMsg(ClientBITags.WrongClientVersion, string.Format("SteamId={0}", this._hmmHub.ClientApi.hubClient.Id), true);
				Guid confirmWindowGuid = Guid.NewGuid();
				ConfirmWindowProperties confirmWindowProperties = new ConfirmWindowProperties
				{
					Guid = confirmWindowGuid,
					TileText = Language.Get("ERROR_VERSION_OUTDATED_TITLE", "Error"),
					QuestionText = Language.Get("ERROR_VERSION_OUTDATED_MESSAGE", "Error").Replace("\\n", "\n"),
					OkButtonText = Language.Get("Ok", "GUI"),
					OnOk = delegate()
					{
						this.HideConfirmationWindow(confirmWindowGuid);
						this._hmmHub.Quit();
					}
				};
				this.OpenConfirmWindow(confirmWindowProperties);
			}
		}

		private void SteamLoginFail(object state, Exception exception)
		{
			Login.Log.Error("Failed!!! Exception:" + exception);
			this._hmmHub.Swordfish.Log.BILogFunnel(FunnelBITags.SplashLoginFailure, null);
			this._loginEndedCallback(false);
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties confirmWindowProperties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.Get("ConnectionTimeOut", "Login").Replace("\\n", "\n"),
				OkButtonText = Language.Get("Ok", "GUI"),
				OnOk = delegate()
				{
					this.HideConfirmationWindow(confirmWindowGuid);
					this._hmmHub.Quit();
				}
			};
			this.OpenConfirmWindow(confirmWindowProperties);
		}

		private void GetSwordfishSessionId()
		{
			this._hmmHub.UserService.GetMySessionId(null, delegate(object state, string swordfishSessionId)
			{
				this.StartGetLoginData();
				this.GetSWFVersion();
			}, delegate(object state, Exception exception)
			{
				this.OnError(exception, "Failed to get swordfishSessionId", 700, default(Guid));
			});
		}

		private void HideConfirmationWindow(Guid confirmWindowGuid)
		{
			this._hmmHub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
		}

		private void StartGetLoginData()
		{
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties confirmWindowProperties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = string.Format(Language.Get("TryingtoGetPlayer", "Login"), new object[0])
			};
			this.OpenConfirmWindow(confirmWindowProperties);
			this._hmmHub.User.GetLoginData(delegate
			{
				this.ConnectToMsgHub(confirmWindowGuid);
			}, delegate
			{
				this.OnError(null, "Get player failed.", 702, confirmWindowGuid);
			}, delegate
			{
				this.HideConfirmationWindow(confirmWindowGuid);
			});
		}

		private void GetSWFVersion()
		{
			this._hmmHub.GetSWFVersion(delegate
			{
			}, delegate(object s, Exception e)
			{
			});
		}

		private void ConnectToMsgHub(Guid getPlayerConfirmWindowGuid)
		{
			this.HideConfirmationWindow(getPlayerConfirmWindowGuid);
			this._msgConnectConfirmWindow = Guid.NewGuid();
			ConfirmWindowProperties confirmWindowProperties = new ConfirmWindowProperties
			{
				Guid = this._msgConnectConfirmWindow,
				QuestionText = string.Format(Language.Get("HubConnecting", "Login"), new object[0])
			};
			this.OpenConfirmWindow(confirmWindowProperties);
			this._hmmHub.Swordfish.Msg.Connect();
		}

		private void OnMsgHubConnected(object sender, EventArgs e)
		{
			this.HideConfirmationWindow(this._msgConnectConfirmWindow);
			this._loginEndedCallback(true);
		}

		private void OnMsgHubConnectError(object sender, Exception eventargs)
		{
			this.HideConfirmationWindow(this._msgConnectConfirmWindow);
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties confirmWindowProperties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = string.Format(Language.Get("HubFailedToConnectMessage", "Login"), new object[0]),
				OkButtonText = Language.Get("Ok", "GUI"),
				OnOk = delegate()
				{
					this.HideConfirmationWindow(confirmWindowGuid);
				}
			};
			this.OpenConfirmWindow(confirmWindowProperties);
		}

		[Obsolete]
		private void AllSetForConnection()
		{
			if (!this._hmmHub.Config.GetBoolValue(ConfigAccess.SkipTutorial, false) && !this._hmmHub.TutorialHub.TutorialControllerInstance.HasFinishedTutorial())
			{
				PlayerCustomWS.ClearCurrentServer(delegate(object x, string y)
				{
					this._loginEndedCallback(true);
				}, delegate(object x, Exception e)
				{
					Login.Log.Fatal("Login - AllSetForConnection - Failed to clear bag.", e);
					this._loginEndedCallback(false);
				});
			}
			else
			{
				this._loginEndedCallback(true);
			}
		}

		private void OpenConfirmWindow(ConfirmWindowProperties confirmWindowProperties)
		{
			this._confirmWindowCallback(confirmWindowProperties);
		}

		private void OnError(Exception exception, string logMessage, int errorCode, Guid oldWindowGuid = default(Guid))
		{
			if (oldWindowGuid != Guid.Empty)
			{
				this.HideConfirmationWindow(oldWindowGuid);
			}
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties confirmWindowProperties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.Get("UnhandledException", "Login"),
				OkButtonText = Language.Get("Ok", "GUI"),
				OnOk = delegate()
				{
					this.HideConfirmationWindow(confirmWindowGuid);
					this._hmmHub.Quit();
				}
			};
			this.OpenConfirmWindow(confirmWindowProperties);
			Login.Log.Error(errorCode + " - " + logMessage, exception);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(Login));

		private Guid _msgConnectConfirmWindow;

		private Action<bool> _loginEndedCallback;

		private Action<ConfirmWindowProperties> _confirmWindowCallback;

		private HMMHub _hmmHub;
	}
}
