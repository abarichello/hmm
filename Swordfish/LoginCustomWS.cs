using System;
using ClientAPI;
using Pocketverse;

namespace HeavyMetalMachines.Swordfish
{
	public class LoginCustomWS : GameHubObject
	{
		public static void GetLoginData(object state, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(state, "GetLoginData", string.Empty, onSuccess, onError);
		}
	}
}
