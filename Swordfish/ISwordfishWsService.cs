using System;
using ClientAPI;
using Hoplon.Serialization;
using UniRx;

namespace HeavyMetalMachines.Swordfish
{
	public interface ISwordfishWsService
	{
		IObservable<T> ExecuteSerializable<T>(Action<SwordfishClientApi.ParameterizedCallback<string>, SwordfishClientApi.ErrorCallback> swordfishCall) where T : JsonSerializeable<T>, new();

		IObservable<Unit> Execute(Action<SwordfishClientApi.Callback, SwordfishClientApi.ErrorCallback> swordfishCall);

		IObservable<Unit> Execute(Action<SwordfishClientApi.ParameterizedCallback<string>, SwordfishClientApi.ErrorCallback> swordfishCall);

		IObservable<T> Execute<T>(Action<SwordfishClientApi.ParameterizedCallback<T>, SwordfishClientApi.ErrorCallback> swordfishCall);

		IObservable<T> ExecuteWithTimeout<T>(Action<SwordfishClientApi.ParameterizedCallback<T>, SwordfishClientApi.ErrorCallback> swordfishCall);

		IObservable<Unit> OnTimeout();

		IObservable<Unit> OnErrorTimeout();
	}
}
