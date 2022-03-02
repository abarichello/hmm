using System;
using ClientAPI;
using ClientAPI.Service.Interfaces;
using Hoplon.Serialization;
using UniRx;

namespace HeavyMetalMachines.Swordfish
{
	public class CustomWsService : ICustomWsService
	{
		public CustomWsService(ICustomWS customWs, ISwordfishWsService swordfishWsService)
		{
			this._customWs = customWs;
			this._swordfishWsService = swordfishWsService;
		}

		public IObservable<TResultType> Execute<TResultType>(string methodName, string argument) where TResultType : JsonSerializeable<TResultType>, new()
		{
			return Observable.TakeUntil<TResultType, Unit>(this.ExecuteCall<TResultType>(methodName, argument), this._swordfishWsService.OnTimeout());
		}

		public IObservable<TResultType> ExecuteWithTimeout<TResultType>(string methodName, string argument) where TResultType : JsonSerializeable<TResultType>, new()
		{
			return Observable.TakeUntil<TResultType, Unit>(this.ExecuteCall<TResultType>(methodName, argument), this._swordfishWsService.OnErrorTimeout());
		}

		private IObservable<TResultType> ExecuteCall<TResultType>(string methodName, string argument) where TResultType : JsonSerializeable<TResultType>, new()
		{
			return SwordfishObservable.FromStringSwordfishCall<TResultType>(delegate(SwordfishClientApi.ParameterizedCallback<string> success, SwordfishClientApi.ErrorCallback error)
			{
				this._customWs.ExecuteCustomWSWithReturn(null, methodName, argument, success, error);
			});
		}

		private readonly ICustomWS _customWs;

		private readonly ISwordfishWsService _swordfishWsService;
	}
}
