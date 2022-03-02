using System;
using ClientAPI.Objects;
using HeavyMetalMachines.DataTransferObjects.Server;
using UniRx;

namespace HeavyMetalMachines.Storyteller
{
	public interface IStorytellerGameserverSearch
	{
		IObservable<GameServerRunningInfo[]> SearchServer(MatchSearchBag search);
	}
}
