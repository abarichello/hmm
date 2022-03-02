using System;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Publishing;
using Pocketverse;

namespace HeavyMetalMachines.Login
{
	public class SwordfishPublisherService : GameHubObject, IGetCurrentPublisher
	{
		public SwordfishPublisherService(ILogin clientApiLoginService)
		{
			this._clientApiLoginService = clientApiLoginService;
			this._cachedPublisher = null;
		}

		public Publisher Get()
		{
			if (this._cachedPublisher == null)
			{
				string currentPublisher = this._clientApiLoginService.GetCurrentPublisher();
				this._cachedPublisher = Publishers.GetPublisherByName(currentPublisher);
			}
			return this._cachedPublisher;
		}

		private readonly ILogin _clientApiLoginService;

		private Publisher _cachedPublisher;
	}
}
