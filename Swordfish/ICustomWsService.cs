using System;
using Hoplon.Serialization;
using UniRx;

namespace HeavyMetalMachines.Swordfish
{
	public interface ICustomWsService
	{
		IObservable<TResultType> Execute<TResultType>(string methodName, string argument) where TResultType : JsonSerializeable<TResultType>, new();

		IObservable<TResultType> ExecuteWithTimeout<TResultType>(string methodName, string argument) where TResultType : JsonSerializeable<TResultType>, new();
	}
}
