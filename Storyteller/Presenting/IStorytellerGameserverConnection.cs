using System;
using ClientAPI.Objects;
using UniRx;

namespace HeavyMetalMachines.Storyteller.Presenting
{
	public interface IStorytellerGameserverConnection
	{
		IObservable<GameServerRunningInfo> OnConnectToMatch();
	}
}
