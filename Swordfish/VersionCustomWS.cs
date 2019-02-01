using System;
using ClientAPI;
using Pocketverse;

namespace HeavyMetalMachines.Swordfish
{
	public class VersionCustomWS : GameHubObject
	{
		public static void GetVersion(SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetVersion", string.Empty, onSuccess, onError);
		}
	}
}
