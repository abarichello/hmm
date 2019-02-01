using System;
using ClientAPI;
using Pocketverse;

namespace HeavyMetalMachines.Swordfish
{
	public class StoreCustomWS : GameHubObject
	{
		public static void SyncServerTime(SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "SyncServerTime", string.Empty, onSuccess, onError);
		}

		public static void ConsumeAccountItemType(string itemTypeid, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "ConsumeAccountItemType", itemTypeid, onSuccess, onError);
		}

		public static void BuyFreeDailyBooster(SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "BuyFreeDailyBooster", string.Empty, onSuccess, onError);
		}
	}
}
