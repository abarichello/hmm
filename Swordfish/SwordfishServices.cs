using System;
using System.Runtime.CompilerServices;
using Assets.Standard_Assets.Scripts.Infra;
using ClientAPI;
using ClientAPI.MessageHub;
using ClientAPI.Service;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.GameServer;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.MatchMaking;
using HeavyMetalMachines.ParentalControl;
using HeavyMetalMachines.Regions.Business;
using HeavyMetalMachines.Social.Groups.Business;
using HeavyMetalMachines.Swordfish.API;
using Hoplon.Input.Business;
using Hoplon.ToggleableFeatures;
using Pocketverse;
using Swordfish.Common.exceptions;
using Zenject;

namespace HeavyMetalMachines.Swordfish
{
	public class SwordfishServices : GameHubBehaviour, MatchController.GameOverMessage.IGameOverListener
	{
		public void Initialize()
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				base.enabled = false;
			}
			try
			{
				this.MatchBI = new SwordfishMatchBI(this._inputBI);
				this.Connection = new SwordfishConnection(this._setServerRegion, this._publisher, this._getParentalControlSettings, this._parseGameServerStartRequest, this._gameServerStartRequestStorage, this._container);
				this.Log = new SwordfishLog();
				if (GameHubBehaviour.Hub.Net.IsClient())
				{
					this.Msg = new SwordfishMessage(this._isFeatureToggled);
					this.Connection.ListenToSwordfishConnected += this.Log.OnListenToSwordfishConnected;
					GameHubBehaviour.Hub.ClientApi.hubClient.Disconnected += this.HubClientOnDisconnected;
					GameHubBehaviour.Hub.ClientApi.hubClient.ConnectionInstability += new EventHandlerEx<ConnectionInstabilityMessage>(this.HubClientOnConnectionInstability);
					SwordfishClientApi clientApi = GameHubBehaviour.Hub.ClientApi;
					if (SwordfishServices.<>f__mg$cache0 == null)
					{
						SwordfishServices.<>f__mg$cache0 = new EventHandler<ApiRateLimitExceededArgs>(SwordfishServices.ShowConnectionLostBecauseApiRateLimitExceeded);
					}
					clientApi.PublisherApiRateLimitExceeded += SwordfishServices.<>f__mg$cache0;
					this._xBoxSessionPublisher = this._container.Resolve<IGroupXBoxSessionPublisher>();
					this._xBoxSessionPublisher.Setup(GameHubBehaviour.Hub.ClientApi, GameHubBehaviour.Hub.GroupService, this._container.Resolve<IGetThenObserveMatchmakingQueueState>());
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
			SwordfishServices.Logger.Debug("Initialize");
		}

		private void OnDestroy()
		{
			SwordfishServices.Logger.Debug("OnDestroy");
			GameHubBehaviour.Hub.ClientApi.WebServiceRequestTimeout -= this.WebServiceTimeOut;
			if (this._xBoxSessionPublisher != null)
			{
				this._xBoxSessionPublisher.Dispose();
			}
		}

		private void Update()
		{
			if (this.Connection == null)
			{
				return;
			}
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
				GameHubBehaviour.Hub.ClientApi.hubClient.Disconnected -= this.HubClientOnDisconnected;
				GameHubBehaviour.Hub.ClientApi.hubClient.ConnectionInstability -= new EventHandlerEx<ConnectionInstabilityMessage>(this.HubClientOnConnectionInstability);
			}
		}

		public void OnGameOver(MatchController.GameOverMessage msg)
		{
			this.MatchBI.GameOver(msg);
		}

		private void WebServiceTimeOut(object state, WebServiceRequestTimeoutArgs args)
		{
			SwordfishServices.Logger.WarnFormat("[WebServiceTimeOut] Api={0} Service={1} State={2}", new object[]
			{
				args.ApiName,
				args.ServiceName,
				args.State
			});
		}

		private void HubClientOnConnectionInstability(object state, ConnectionInstabilityMessage eventargs)
		{
			SwordfishServices.Logger.WarnFormat("HubClientOnConnectionInstability: Type={0} MessageId={1} DisconnectedRegion={2}", new object[]
			{
				eventargs.Type,
				eventargs.MessageId.ToString(),
				eventargs.DisconnectedRegion
			});
		}

		private void HubClientOnDisconnected(object state, DisconnectionReasonWrapper e)
		{
			SwordfishServices.Logger.WarnFormat("HubClientOnDisconnected: Reason={0} Ex={1}", new object[]
			{
				e.GetReason(),
				e.GetException()
			});
		}

		private static void ShowConnectionLostBecauseApiRateLimitExceeded(object _, ApiRateLimitExceededArgs args)
		{
			SwordfishServices.Logger.WarnFormat("The publisher API rate limit exceeded. API={0}", new object[]
			{
				args.ApiName
			});
			GameHubBehaviour.Hub.Swordfish.Connection.ShowConnectionLost("The publisher API rate limit exceeded");
		}

		private static void ShowExceptionFeedback(Exception e)
		{
			string questionText;
			if (e is InvalidOperationException)
			{
				if (e.Message == "Steamworks is not initialized.")
				{
					SwordfishServices.Logger.Fatal("InvalidOperationException: Steam not initialized.", e);
					questionText = Language.Get("InvalidOperationException_SteamNoInitialized", TranslationContext.GUI);
				}
				else
				{
					SwordfishServices.Logger.Error("InvalidOperationException: Erro desconhecido.");
					questionText = Language.Get("InvalidOperationException_ErroDesconhecido", TranslationContext.GUI);
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
						questionText = Language.Get("LoginFailedException_PublisherUserCouldNotBeCreated", TranslationContext.GUI);
						goto IL_1E2;
					}
					if (message == "User is banned.")
					{
						SwordfishServices.Logger.Error("LoginFailedException: User is banned.");
						questionText = Language.Get("LoginFailedException_UserIsBanned", TranslationContext.GUI);
						goto IL_1E2;
					}
					if (message == "Steam ticket was not informed!")
					{
						SwordfishServices.Logger.Error("LoginFailedException: Steam ticket was not informed!");
						questionText = Language.Get("LoginFailedException_SteamTicketWasNotInformed", TranslationContext.GUI);
						goto IL_1E2;
					}
					if (message == "Publisher return - User is Offline")
					{
						SwordfishServices.Logger.Error("LoginFailedException: Publisher return - User is Offline");
						questionText = Language.Get("LoginFailedException_UserIsOffline", TranslationContext.GUI);
						goto IL_1E2;
					}
					if (message == "Publisher return - Invalid Ticket")
					{
						SwordfishServices.Logger.Error("LoginFailedException: Publisher return - Invalid Ticket");
						questionText = Language.Get("LoginFailedException_InvalidTicket", TranslationContext.GUI);
						goto IL_1E2;
					}
					if (message == "Steam web API is offline")
					{
						SwordfishServices.Logger.Error("LoginFailedException: Steam web API is offline");
						questionText = Language.Get("LoginFailedException_SteamWebAPIIsOffline", TranslationContext.GUI);
						goto IL_1E2;
					}
				}
				SwordfishServices.Logger.Error("LoginFailedException: Erro desconhecido.");
				questionText = Language.Get("LoginFailedException_ErroDesconhecido", TranslationContext.GUI);
				IL_1E2:;
			}
			else if (e is OutOfServiceException)
			{
				if (e.Message == "Publisher is out of service.")
				{
					SwordfishServices.Logger.Error("OutOfServiceException: Publisher is out of service.");
					questionText = Language.Get("OutOfServiceException_PublisherIsOutOfService", TranslationContext.GUI);
				}
				else
				{
					SwordfishServices.Logger.Error("OutOfServiceException: Erro desconhecido.");
					questionText = Language.Get("OutOfServiceException_ErroDesconhecido", TranslationContext.GUI);
				}
			}
			else
			{
				SwordfishServices.Logger.Fatal("Exception: Erro desconhecido.", e);
				questionText = Language.Get("Exception_ErroDesconhecido", TranslationContext.GUI);
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
			confirmWindowProperties.OkButtonText = Language.Get("Ok", TranslationContext.GUI);
			confirmWindowProperties.OnOk = delegate()
			{
				SwordfishServices.Logger.Debug("ShowExceptionFeedbackResult. Calling EndSession");
				GameHubBehaviour.Hub.EndSession("SwordfishServices.InitializeException");
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(confirmWindowProperties);
		}

		public static readonly BitLogger Logger = new BitLogger(typeof(SwordfishServices));

		public SwordfishLog Log;

		public SwordfishMessage Msg;

		public SwordfishConnection Connection;

		public ISwordfishMatchBI MatchBI;

		[Inject]
		private IPublisher _publisher;

		[Inject]
		private IIsFeatureToggled _isFeatureToggled;

		[Inject]
		private DiContainer _container;

		[InjectOnClient]
		private IInputBI _inputBI;

		[InjectOnClient]
		private IGetParentalControlSettings _getParentalControlSettings;

		private IGroupXBoxSessionPublisher _xBoxSessionPublisher;

		[InjectOnServer]
		private ISetServerRegion _setServerRegion;

		[InjectOnServer]
		private IParseGameServerStartRequest _parseGameServerStartRequest;

		[InjectOnServer]
		private IGameServerStartRequestStorage _gameServerStartRequestStorage;

		[CompilerGenerated]
		private static EventHandler<ApiRateLimitExceededArgs> <>f__mg$cache0;
	}
}
