using System;
using ClientAPI;
using ClientAPI.Exceptions;
using ClientAPI.Objects;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.Login.DataTransferObjects;
using HeavyMetalMachines.Login.Exceptions;
using HeavyMetalMachines.ParentalControl;
using HeavyMetalMachines.ReportSystem.DataTransferObjects;
using HeavyMetalMachines.ReportSystem.Exceptions;
using HeavyMetalMachines.Swordfish;
using Hoplon.Logging;
using Hoplon.Serialization;
using Pocketverse;
using Swordfish.Common.exceptions;
using UniRx;

namespace HeavyMetalMachines.Login
{
	public class SwordfishBackendLoginService : IBackendLoginService
	{
		public SwordfishBackendLoginService(ILogin loginService, IUser userService, ICustomWsService customWsService, SwordfishClientApi clientApi, IGenerateClientLoginRequest generateClientLoginRequest, UserInfo userInfo, SwordfishConnection swordfishConnection, IConfigLoader configLoader, ISwordfishWsService swordfishWsService, ICheckPlatformPrivileges checkPlatformPrivileges, ILogger<SwordfishBackendLoginService> logger)
		{
			this._loginService = loginService;
			this._userService = userService;
			this._customWsService = customWsService;
			this._clientApi = clientApi;
			this._generateClientLoginRequest = generateClientLoginRequest;
			this._userInfo = userInfo;
			this._swordfishConnection = swordfishConnection;
			this._configLoader = configLoader;
			this._swordfishWsService = swordfishWsService;
			this._checkPlatformPrivileges = checkPlatformPrivileges;
			this._logger = logger;
		}

		public IObservable<BackendSession> Login()
		{
			return Observable.Select<LoginInfo, BackendSession>(Observable.Do<LoginInfo>(Observable.DoOnError<LoginInfo>(this.ExecuteSwordfishLogin(), new Action<Exception>(this.LogLoginErrorAndThrowBusinessException)), new Action<LoginInfo>(this.LogLoginSuccess)), new Func<LoginInfo, BackendSession>(this.ConvertToBackendSession));
		}

		private IObservable<LoginInfo> ExecuteSwordfishLogin()
		{
			return this._swordfishWsService.ExecuteWithTimeout<LoginInfo>(delegate(SwordfishClientApi.ParameterizedCallback<LoginInfo> success, SwordfishClientApi.ErrorCallback error)
			{
				SerializableClientLoginRequest serializableClientLoginRequest = this._generateClientLoginRequest.Generate();
				this._loginService.DoLogin(null, success, error, serializableClientLoginRequest.Serialize());
			});
		}

		private void LogLoginSuccess(LoginInfo loginInfo)
		{
			this._logger.DebugFormat("Connected to Swordfish. SessionID={0}", new object[]
			{
				loginInfo.SessionId
			});
			this._swordfishConnection.SessionId = loginInfo.SessionId.ToString();
			this._swordfishConnection.SetIsFirstLogin(loginInfo.IsFirstLogin);
			this._clientApi.RequestTimeOut = this._configLoader.GetIntValue(ConfigAccess.SFTimeout);
		}

		private BackendSession ConvertToBackendSession(LoginInfo loginInfo)
		{
			return new BackendSession
			{
				Id = loginInfo.SessionId,
				IsFirstSession = loginInfo.IsFirstLogin
			};
		}

		private void LogLoginErrorAndThrowBusinessException(Exception exception)
		{
			string text = exception.ToString();
			this._logger.ErrorFormat("Swordfish login failed. Exception={0}", new object[]
			{
				text
			});
			this.CheckClientOnOutdatedPlatformPatch(exception, text);
			this.CheckClientUnableToLoginException(exception, text);
			SwordfishBackendLoginService.CheckTimeoutApiException(exception, text);
			SwordfishBackendLoginService.CheckAccountBanException(exception, text);
		}

		private static void CheckAccountBanException(Exception exception, string exceptionMsg)
		{
			if (exception is AccountBanException)
			{
				AccountBanException ex = (AccountBanException)exception;
				RestrictionFeedbackBag restrictionFeedbackBag = JsonSerializeable<RestrictionFeedbackBag>.Deserialize(ex.Message);
				throw new AccountBannedException(exceptionMsg, restrictionFeedbackBag);
			}
		}

		private static void CheckTimeoutApiException(Exception exception, string exceptionMsg)
		{
			if (exception is TimeoutApiException)
			{
				throw new TimeoutException(exceptionMsg);
			}
		}

		private void CheckClientOnOutdatedPlatformPatch(Exception exception, string exceptionMsg)
		{
		}

		private void CheckClientUnableToLoginException(Exception exception, string exceptionMsg)
		{
			if (exception is ClientUnableToLoginException)
			{
				ClientUnableToLoginException ex = (ClientUnableToLoginException)exception;
				SerializableLoginErrorMessage serializableLoginErrorMessage = JsonSerializeable<SerializableLoginErrorMessage>.Deserialize(ex.Message);
				if (serializableLoginErrorMessage.ErrorCode == 0)
				{
					throw new OutdatedVersionException(exceptionMsg);
				}
				this._logger.Debug("passou CheckClientUnableToLoginException ClientUnableToLoginException");
				string message = string.Format("ClientUnableToLoginException Unkown ErrorCode:{0}. Exception:{1}", serializableLoginErrorMessage.ErrorCode, exceptionMsg);
				throw new Exception(message);
			}
			else
			{
				if (exception is MultiplayerSessionNotAllowedException)
				{
					this._logger.Debug("passou CheckClientUnableToLoginException OnlineServicesBlockedException");
					ObservableExtensions.Subscribe<Unit>(this.Logout());
					throw new OnlineServicesBlockedException(exceptionMsg);
				}
				return;
			}
		}

		public IObservable<Unit> GetLoginData()
		{
			return Observable.AsUnitObservable<NetResult>(Observable.DoOnError<NetResult>(Observable.Do<NetResult>(this._customWsService.ExecuteWithTimeout<NetResult>("GetLoginData", string.Empty), new Action<NetResult>(this.FillLoginData)), new Action<Exception>(this.LogLoginDataError)));
		}

		public void CancelLogin()
		{
			this._loginService.CancelLogin();
		}

		private void FillLoginData(NetResult result)
		{
			if (!result.Success)
			{
				throw new Exception(string.Format("Failed to get login data. {0}", result.Msg));
			}
			this._userInfo.SetLoginData((LoginData)((JsonSerializeable<!0>)result.Msg));
		}

		private void LogLoginDataError(Exception exception)
		{
			this._logger.Error(string.Format("An error occurred while getting login data. {0}", exception));
		}

		private IObservable<Unit> Logout()
		{
			return Observable.AsUnitObservable<bool>(Observable.DoOnError<bool>(Observable.DoOnCompleted<bool>(Observable.DoOnSubscribe<bool>(SwordfishObservable.FromSwordfishCall<bool>(delegate(SwordfishClientApi.ParameterizedCallback<bool> success, SwordfishClientApi.ErrorCallback error)
			{
				this._loginService.DoLogout(null, success, error);
			}), delegate()
			{
				this._logger.Info("Started fallback Swordfish Logout.");
			}), delegate()
			{
				this._logger.Info("Finished fallback Swordfish Logout.");
			}), delegate(Exception error)
			{
				this._logger.InfoFormat("Error on fallback Swordfish Logout. Error={0}", new object[]
				{
					error
				});
			}));
		}

		private readonly ILogin _loginService;

		private readonly IUser _userService;

		private readonly ICustomWsService _customWsService;

		private readonly SwordfishClientApi _clientApi;

		private readonly IGenerateClientLoginRequest _generateClientLoginRequest;

		private readonly UserInfo _userInfo;

		private readonly SwordfishConnection _swordfishConnection;

		private readonly IConfigLoader _configLoader;

		private readonly ISwordfishWsService _swordfishWsService;

		private readonly ICheckPlatformPrivileges _checkPlatformPrivileges;

		private readonly ILogger<SwordfishBackendLoginService> _logger;
	}
}
