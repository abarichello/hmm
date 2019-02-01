using System;
using ClientAPI.Service;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Match;
using Pocketverse;
using Steamworks;
using Swordfish.Common.exceptions;

namespace HeavyMetalMachines.Swordfish
{
	public class SwordfishServices : GameHubBehaviour, MatchController.GameOverMessage.IGameOverListener
	{
		private void OnEnable()
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				base.enabled = false;
			}
			if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish) && GameHubBehaviour.Hub.Net.IsClient() && !SteamAPI.IsSteamRunning())
			{
				SwordfishServices.Logger.ErrorFormat("Steam is not running -> Show feedback and Quit Application", new object[0]);
				SwordfishServices.ShowExceptionFeedbackResult(Language.Get("InvalidOperationException_SteamNoInitialized", "GUI"));
			}
			try
			{
				this.MatchBI = new SwordfishMatchBI();
				this.Connection = new SwordfishConnection();
				this.Log = new SwordfishLog();
				if (GameHubBehaviour.Hub.Net.IsClient())
				{
					this.Msg = new SwordfishMessage();
					this.Connection.ListenToSwordfishConnected += this.Log.OnListenToSwordfishConnected;
				}
				else
				{
					MatchLogWriter.LogStart();
				}
			}
			catch (Exception e)
			{
				SwordfishServices.ShowExceptionFeedback(e);
			}
			GameHubBehaviour.Hub.ClientApi.WebServiceRequestTimeout += this.WebServiceTimeOut;
			if (!string.IsNullOrEmpty(this._serializationHackSessionId))
			{
				this.Connection.SessionId = this._serializationHackSessionId;
			}
		}

		private void Cleanup()
		{
			if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish) && GameHubBehaviour.Hub.Net.IsClient())
			{
				GameHubBehaviour.Hub.Swordfish.Msg.Cleanup();
			}
		}

		private void OnDestroy()
		{
			GameHubBehaviour.Hub.ClientApi.WebServiceRequestTimeout -= this.WebServiceTimeOut;
			this.Cleanup();
		}

		private void Update()
		{
			this.Connection.Update();
			this.MatchBI.Update();
			this.Log.Update();
		}

		private void OnDisable()
		{
			if (this.Connection == null)
			{
				SwordfishServices.Logger.Warn("Swordfish Connection was null, probably steam is not running.");
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				this.Connection.ListenToSwordfishConnected -= this.Log.OnListenToSwordfishConnected;
			}
			this._serializationHackSessionId = this.Connection.SessionId;
		}

		public void OnGameOver(MatchController.GameOverMessage msg)
		{
			this.MatchBI.GameOver(msg);
		}

		private void WebServiceTimeOut(object state, WebServiceRequestTimeoutArgs args)
		{
			ISwordfishWebServiceTimeOut swordfishWebServiceTimeOut = args.State as ISwordfishWebServiceTimeOut;
			if (swordfishWebServiceTimeOut == null)
			{
				return;
			}
			SwordfishServices.Logger.ErrorFormat(string.Format("[WebServiceTimeOut] {0}, {1}, {2}, {3} ", new object[]
			{
				swordfishWebServiceTimeOut.TimeOutMessage(),
				args.ApiName,
				args.ServiceName,
				args.State
			}), new object[0]);
			swordfishWebServiceTimeOut.TimeOut();
		}

		private static void ShowExceptionFeedback(Exception e)
		{
			string questionText;
			if (e is InvalidOperationException)
			{
				if (e.Message == "Steamworks is not initialized.")
				{
					SwordfishServices.Logger.Fatal("InvalidOperationException: Steam not initialized.", e);
					questionText = Language.Get("InvalidOperationException_SteamNoInitialized", "GUI");
				}
				else
				{
					SwordfishServices.Logger.Error("InvalidOperationException: Erro desconhecido.");
					questionText = Language.Get("InvalidOperationException_ErroDesconhecido", "GUI");
				}
			}
			else if (e is LoginFailedException)
			{
				string message = e.Message;
				if (message != null)
				{
					if (message == "Publisher User could not be created")
					{
						SwordfishServices.Logger.Error("LoginFailedException: Publisher User could not be created.");
						questionText = Language.Get("LoginFailedException_PublisherUserCouldNotBeCreated", "GUI");
						goto IL_1E2;
					}
					if (message == "User is banned.")
					{
						SwordfishServices.Logger.Error("LoginFailedException: User is banned.");
						questionText = Language.Get("LoginFailedException_UserIsBanned", "GUI");
						goto IL_1E2;
					}
					if (message == "Steam ticket was not informed!")
					{
						SwordfishServices.Logger.Error("LoginFailedException: Steam ticket was not informed!");
						questionText = Language.Get("LoginFailedException_SteamTicketWasNotInformed", "GUI");
						goto IL_1E2;
					}
					if (message == "Publisher return - User is Offline")
					{
						SwordfishServices.Logger.Error("LoginFailedException: Publisher return - User is Offline");
						questionText = Language.Get("LoginFailedException_UserIsOffline", "GUI");
						goto IL_1E2;
					}
					if (message == "Publisher return - Invalid Ticket")
					{
						SwordfishServices.Logger.Error("LoginFailedException: Publisher return - Invalid Ticket");
						questionText = Language.Get("LoginFailedException_InvalidTicket", "GUI");
						goto IL_1E2;
					}
					if (message == "Steam web API is offline")
					{
						SwordfishServices.Logger.Error("LoginFailedException: Steam web API is offline");
						questionText = Language.Get("LoginFailedException_SteamWebAPIIsOffline", "GUI");
						goto IL_1E2;
					}
				}
				SwordfishServices.Logger.Error("LoginFailedException: Erro desconhecido.");
				questionText = Language.Get("LoginFailedException_ErroDesconhecido", "GUI");
				IL_1E2:;
			}
			else if (e is OutOfServiceException)
			{
				if (e.Message == "Publisher is out of service.")
				{
					SwordfishServices.Logger.Error("OutOfServiceException: Publisher is out of service.");
					questionText = Language.Get("OutOfServiceException_PublisherIsOutOfService", "GUI");
				}
				else
				{
					SwordfishServices.Logger.Error("OutOfServiceException: Erro desconhecido.");
					questionText = Language.Get("OutOfServiceException_ErroDesconhecido", "GUI");
				}
			}
			else
			{
				SwordfishServices.Logger.Fatal("Exception: Erro desconhecido.", e);
				questionText = Language.Get("Exception_ErroDesconhecido", "GUI");
			}
			SwordfishServices.Logger.ErrorFormat("Exception Result Message: {0}", new object[]
			{
				e.Message
			});
			SwordfishServices.ShowExceptionFeedbackResult(questionText);
		}

		private static void ShowExceptionFeedbackResult(string questionText)
		{
			ConfirmWindowProperties confirmWindowProperties = new ConfirmWindowProperties();
			confirmWindowProperties.Guid = Guid.NewGuid();
			confirmWindowProperties.QuestionText = questionText;
			confirmWindowProperties.OkButtonText = Language.Get("Ok", "GUI");
			confirmWindowProperties.OnOk = delegate()
			{
				try
				{
					if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
					{
						GameHubBehaviour.Hub.Swordfish.Msg.Cleanup();
					}
				}
				catch (Exception ex)
				{
				}
				GameHubBehaviour.Hub.Quit();
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(confirmWindowProperties);
		}

		public static readonly BitLogger Logger = new BitLogger(typeof(SwordfishServices));

		public SwordfishLog Log;

		public SwordfishMessage Msg;

		public SwordfishConnection Connection;

		public SwordfishMatchBI MatchBI;

		private string _serializationHackSessionId;
	}
}
