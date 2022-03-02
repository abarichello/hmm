using System;
using ClientAPI.Objects;
using HeavyMetalMachines.DataTransferObjects.Server;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using UniRx;

namespace HeavyMetalMachines.Storyteller
{
	public class StorytellerGameserverSearch : IStorytellerGameserverSearch
	{
		public IObservable<GameServerRunningInfo[]> SearchServer(MatchSearchBag search)
		{
			return this._service.SearchServer(search);
		}

		[InjectOnClient]
		private IStorytellerGameserverSearchService _service;
	}
}
