using System;
using ClientAPI;
using ClientAPI.Objects;
using ClientAPI.Service.Interfaces;
using Pocketverse;

namespace HeavyMetalMachines.Login
{
	public class SwordfishLoginProxy : GameHubObject, ILogin
	{
		public SwordfishLoginProxy(IConfigLoader configLoader)
		{
			this._configLoader = configLoader;
		}

		public void CancelLogin()
		{
			GameHubObject.Hub.ClientApi.login.CancelLogin();
		}

		public void DoLogin(object state, SwordfishClientApi.ParameterizedCallback<LoginInfo> callback, SwordfishClientApi.ErrorCallback errorCallback, string clientLoginRequestBag = "")
		{
			GameHubObject.Hub.ClientApi.login.DoLogin(state, callback, errorCallback, clientLoginRequestBag);
		}

		public LoginInfo DoLoginSync(string clientBag = "")
		{
			return GameHubObject.Hub.ClientApi.login.DoLoginSync(clientBag);
		}

		public string GetCurrentPublisher()
		{
			return GameHubObject.Hub.ClientApi.login.GetCurrentPublisher();
		}

		public event EventHandler<EventArgs> OnPublisherUserSignOut
		{
			add
			{
				GameHubObject.Hub.ClientApi.login.OnPublisherUserSignOut += value;
			}
			remove
			{
				GameHubObject.Hub.ClientApi.login.OnPublisherUserSignOut -= value;
			}
		}

		public void DoLogout(object state, SwordfishClientApi.ParameterizedCallback<bool> successCallback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			GameHubObject.Hub.ClientApi.login.DoLogout(state, successCallback, errorCallback);
		}

		public bool DoLogoutSync()
		{
			return GameHubObject.Hub.ClientApi.login.DoLogoutSync();
		}

		private readonly IConfigLoader _configLoader;
	}
}
