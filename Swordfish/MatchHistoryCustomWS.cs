using System;
using ClientAPI;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;

namespace HeavyMetalMachines.Swordfish
{
	public class MatchHistoryCustomWS : GameHubObject
	{
		public static void CreateANewMatch(MatchHistoryItemBag matchItem, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "CreateANewMatch", matchItem.ToString(), onSuccess, onError);
		}

		public static void GetMatchHistory(SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetMatchHistory", string.Empty, onSuccess, onError);
		}

		public static void GetLastMatchIdByUniversalId(long universalId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetLastMatchIdByUniversalId", universalId.ToString(), onSuccess, onError);
		}
	}
}
