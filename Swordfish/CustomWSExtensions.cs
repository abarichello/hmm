using System;
using ClientAPI;
using ClientAPI.Service.Interfaces;
using Hoplon.Serialization;
using UniRx;

namespace HeavyMetalMachines.Swordfish
{
	public static class CustomWSExtensions
	{
		public static IObservable<TResultType> ExecuteAsObservable<TResultType>(this ICustomWS customWs, string methodName, string argument) where TResultType : JsonSerializeable<TResultType>, new()
		{
			return SwordfishObservable.FromStringSwordfishCall<TResultType>(delegate(SwordfishClientApi.ParameterizedCallback<string> success, SwordfishClientApi.ErrorCallback error)
			{
				customWs.ExecuteCustomWSWithReturn(null, methodName, argument, success, error);
			});
		}
	}
}
