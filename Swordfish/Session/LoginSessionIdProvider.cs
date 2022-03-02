using System;
using ClientAPI;

namespace HeavyMetalMachines.Swordfish.Session
{
	public class LoginSessionIdProvider : ILoginSessionIdProvider
	{
		public LoginSessionIdProvider(SwordfishClientApi clientApi)
		{
			if (clientApi == null)
			{
				throw new ArgumentNullException("clientApi");
			}
			this._clientApi = clientApi;
		}

		public string GetToken()
		{
			return this._clientApi.Token;
		}

		private readonly SwordfishClientApi _clientApi;
	}
}
