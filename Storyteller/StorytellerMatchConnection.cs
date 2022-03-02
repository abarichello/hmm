using System;
using ClientAPI.Objects;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using UniRx;

namespace HeavyMetalMachines.Storyteller
{
	public class StorytellerMatchConnection : IStorytellerMatchConnection
	{
		public IObservable<bool> ConnectToServer(GameServerRunningInfo server)
		{
			return this._service.ConnectToServer(server);
		}

		[InjectOnClient]
		private IStorytellerMatchConnectionService _service;
	}
}
