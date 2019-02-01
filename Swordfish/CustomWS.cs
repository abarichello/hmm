using System;
using ClientAPI;
using Pocketverse;

namespace HeavyMetalMachines.Swordfish
{
	public class CustomWS : GameHubObject
	{
		public static void TestCustomWS(string test, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "TestCustomWS", test, onSuccess, onError);
		}
	}
}
