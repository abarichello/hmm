using System;
using ClientAPI.Objects;
using UniRx;

namespace HeavyMetalMachines.Storyteller
{
	public interface IStorytellerMatchConnection
	{
		IObservable<bool> ConnectToServer(GameServerRunningInfo server);
	}
}
