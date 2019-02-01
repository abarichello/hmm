using System;
using ClientAPI;
using Pocketverse;

namespace HeavyMetalMachines.Swordfish
{
	public class MainMenuCustomWS : GameHubObject
	{
		public static void GetMainMenuData(object state, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(state, "GetMainMenuData", string.Empty, onSuccess, onError);
		}
	}
}
