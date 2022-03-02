using System;
using ClientAPI;
using ClientAPI.Service;
using HeavyMetalMachines.DataTransferObjects.Result;
using Hoplon.Serialization;
using UniRx;

namespace HeavyMetalMachines.Swordfish
{
	public class SwordfishWsService : ISwordfishWsService
	{
		public SwordfishWsService(SwordfishClientApi clientApi)
		{
			this._clientApi = clientApi;
		}

		public IObservable<T> ExecuteSerializable<T>(Action<SwordfishClientApi.ParameterizedCallback<string>, SwordfishClientApi.ErrorCallback> swordfishCall) where T : JsonSerializeable<T>, new()
		{
			return Observable.TakeUntil<T, Unit>(SwordfishObservable.Create<T, string>(swordfishCall, new Func<string, T>(JsonSerializeable.ToObject<T>)), this.OnTimeout());
		}

		public IObservable<Unit> Execute(Action<SwordfishClientApi.Callback, SwordfishClientApi.ErrorCallback> swordfishCall)
		{
			return SwordfishObservable.FromSwordfishCall(swordfishCall);
		}

		public IObservable<Unit> Execute(Action<SwordfishClientApi.ParameterizedCallback<string>, SwordfishClientApi.ErrorCallback> swordfishCall)
		{
			return Observable.TakeUntil<Unit, Unit>(Observable.AsUnitObservable<string>(Observable.Do<string>(SwordfishObservable.Create<string, string>(swordfishCall, (string result) => result), delegate(string result)
			{
				SwordfishWsService.AssertResultSuccess((NetResult)((JsonSerializeable<!0>)result));
			})), this.OnTimeout());
		}

		public IObservable<T> Execute<T>(Action<SwordfishClientApi.ParameterizedCallback<T>, SwordfishClientApi.ErrorCallback> swordfishCall)
		{
			return Observable.TakeUntil<T, Unit>(SwordfishObservable.FromSwordfishCall<T>(swordfishCall), this.OnTimeout());
		}

		public IObservable<T> ExecuteWithTimeout<T>(Action<SwordfishClientApi.ParameterizedCallback<T>, SwordfishClientApi.ErrorCallback> swordfishCall)
		{
			return Observable.TakeUntil<T, Unit>(SwordfishObservable.FromSwordfishCall<T>(swordfishCall), this.OnErrorTimeout());
		}

		private static void AssertResultSuccess(NetResult result)
		{
			if (!result.Success)
			{
				throw new NetResultException(result.Msg);
			}
		}

		public IObservable<Unit> OnErrorTimeout()
		{
			return Observable.AsUnitObservable<WebServiceRequestTimeoutArgs>(Observable.Do<WebServiceRequestTimeoutArgs>(this.OnTimeoutEvent(), delegate(WebServiceRequestTimeoutArgs args)
			{
				throw new TimeoutException(string.Format("API {0} has timed out.", args.ApiName));
			}));
		}

		public IObservable<Unit> OnTimeout()
		{
			return Observable.AsUnitObservable<WebServiceRequestTimeoutArgs>(this.OnTimeoutEvent());
		}

		private IObservable<WebServiceRequestTimeoutArgs> OnTimeoutEvent()
		{
			return Observable.Select<EventPattern<WebServiceRequestTimeoutArgs>, WebServiceRequestTimeoutArgs>(Observable.FromEventPattern<EventHandler<WebServiceRequestTimeoutArgs>, WebServiceRequestTimeoutArgs>((EventHandler<WebServiceRequestTimeoutArgs> handler) => new EventHandler<WebServiceRequestTimeoutArgs>(handler.Invoke), delegate(EventHandler<WebServiceRequestTimeoutArgs> handler)
			{
				this._clientApi.WebServiceRequestTimeout += handler;
			}, delegate(EventHandler<WebServiceRequestTimeoutArgs> handler)
			{
				this._clientApi.WebServiceRequestTimeout -= handler;
			}), (EventPattern<WebServiceRequestTimeoutArgs> evnt) => evnt.EventArgs);
		}

		private readonly SwordfishClientApi _clientApi;
	}
}
