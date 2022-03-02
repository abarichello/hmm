using System;
using ClientAPI.Objects;
using UniRx;

namespace HeavyMetalMachines.Storyteller
{
	public interface IStorytellerMatchConnectionService
	{
		IObservable<bool> ConnectToServer(GameServerRunningInfo server);
	}
}
