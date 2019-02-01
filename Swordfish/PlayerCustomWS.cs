using System;
using ClientAPI;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;

namespace HeavyMetalMachines.Swordfish
{
	public class PlayerCustomWS : GameHubObject
	{
		public static void SetNickName(string name, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "SetNickName", name, onSuccess, onError);
		}

		public static void GetPlayerFounderLevel(string targetPlayerId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetPlayerFounderLevel", targetPlayerId, onSuccess, onError);
		}

		public static void GetPlayerFounderLevelByUniversalId(string targetUniversalId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetPlayerFounderLevelByUniversalId", targetUniversalId, onSuccess, onError);
		}

		public static void GetPlayerPortraitCustomizationByUniversalId(string targetUniversalId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetPlayerPortraitCustomizationByUniversalId", targetUniversalId, onSuccess, onError);
		}

		public static void CheckNickName(string name, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "CheckNickName", name, onSuccess, onError);
		}

		public static void ClearCurrentServer(SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "ClearCurrentServer", string.Empty, onSuccess, onError);
		}

		public static void GetPlayerLegacyLevel(string universalId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetPlayerLegacyLevel", universalId, onSuccess, onError);
		}

		public static void GetPlayerBattlepassLevel(string universalId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetPlayerBattlepassLevel", universalId, onSuccess, onError);
		}

		public static void GetPlayerTotalLevelByUserId(string universalId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetPlayerTotalLevelByUserId", universalId, onSuccess, onError);
		}

		public static void IncrementPlayerEndMatchPresenceCounter(string universalId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "IncrementPlayerEndMatchPresenceCounter", universalId, onSuccess, onError);
		}

		public static void MarkPlayerHasDoneTutorial(string universalId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "MarkPlayerHasDoneTutorial", universalId, onSuccess, onError);
		}

		public static void SaveTutorialSteps(string tutorialSteps, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "SaveTutorialSteps", TutorialUtils.ToBase64(tutorialSteps), onSuccess, onError);
		}

		public static void UpdatePlayerUnlockMask(PlayerBag bag, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "UpdatePlayerUnlockMask", bag.ToString(), onSuccess, onError);
		}

		public static void SavePlayerPrefs(string prefs, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "SavePlayerPrefs", prefs, onSuccess, onError);
		}

		public static void UpdatePlayerNews(PlayerBag bag, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "UpdatePlayerNews", bag.ToString(), onSuccess, onError);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PlayerCustomWS));
	}
}
