using System;
using ClientAPI;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Swordfish;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.Login
{
	public class SwordfishBackendLogoutService : IBackendLogoutService
	{
		public SwordfishBackendLogoutService(ILogin loginService, ILogger<SwordfishBackendLogoutService> logger)
		{
			this._loginService = loginService;
			this._logger = logger;
		}

		public IObservable<Unit> Logout()
		{
			return Observable.AsUnitObservable<bool>(Observable.DoOnError<bool>(Observable.DoOnCompleted<bool>(Observable.DoOnSubscribe<bool>(SwordfishObservable.FromSwordfishCall<bool>(delegate(SwordfishClientApi.ParameterizedCallback<bool> success, SwordfishClientApi.ErrorCallback error)
			{
				this._loginService.DoLogout(null, success, error);
			}), delegate()
			{
				this._logger.Info("Started Swordfish Logout.");
			}), delegate()
			{
				this._logger.Info("Finished Swordfish Logout.");
			}), delegate(Exception error)
			{
				this._logger.InfoFormat("Error on Swordfish Logout. Error={0}", new object[]
				{
					error
				});
			}));
		}

		private readonly ILogin _loginService;

		private readonly ILogger<SwordfishBackendLogoutService> _logger;
	}
}
