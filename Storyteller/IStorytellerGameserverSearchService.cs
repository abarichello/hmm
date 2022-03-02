using System;
using ClientAPI.Objects;
using HeavyMetalMachines.DataTransferObjects.Server;
using UniRx;

namespace HeavyMetalMachines.Storyteller
{
	public interface IStorytellerGameserverSearchService
	{
		IObservable<GameServerRunningInfo[]> SearchServer(MatchSearchBag search);
	}
}
