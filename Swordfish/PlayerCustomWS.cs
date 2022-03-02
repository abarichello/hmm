using System;
using ClientAPI;
using HeavyMetalMachines.DataTransferObjects.Player;
using Pocketverse;

namespace HeavyMetalMachines.Swordfish
{
	public class PlayerCustomWS : GameHubObject
	{
		public static void SetNickName(string name, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			PlayerCustomWS.Log.DebugFormat("SetNickName {0}", new object[]
			{
				name
			});
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "SetNickName", name, onSuccess, onError);
		}

		public static void GetPlayerFounderLevel(string targetPlayerId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			PlayerCustomWS.Log.DebugFormat("GetPlayerFounderLevel {0}", new object[]
			{
				targetPlayerId
			});
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetPlayerFounderLevel", targetPlayerId, onSuccess, onError);
		}

		public static void GetPlayerFounderLevelByUniversalId(string targetUniversalId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			PlayerCustomWS.Log.DebugFormat("GetPlayerFounderLevelByUniversalId {0}", new object[]
			{
				targetUniversalId
			});
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetPlayerFounderLevelByUniversalId", targetUniversalId, onSuccess, onError);
		}

		public static void GetPlayerPortraitCustomizationByUniversalId(string targetUniversalId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			PlayerCustomWS.Log.DebugFormat("GetPlayerPortraitCustomizationByUniversalId {0}", new object[]
			{
				targetUniversalId
			});
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetPlayerPortraitCustomizationByUniversalId", targetUniversalId, onSuccess, onError);
		}

		public static void CheckNickName(string name, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			PlayerCustomWS.Log.DebugFormat("CheckNickName {0}", new object[]
			{
				name
			});
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "CheckNickName", name, onSuccess, onError);
		}

		public static void ClearCurrentServerForPlayer(long playerId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			PlayerCustomWS.Log.DebugFormat("ClearCurrentServerForPlayer {0}", new object[]
			{
				playerId
			});
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "ClearCurrentServerForPlayer", playerId.ToString(), onSuccess, onError);
		}

		public static void ClearCurrentServer(SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			PlayerCustomWS.Log.Debug("ClearCurrentServer");
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "ClearCurrentServer", string.Empty, onSuccess, onError);
		}

		public static void GetPlayerLegacyLevel(string universalId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			PlayerCustomWS.Log.Debug("GetPlayerLegacyLevel");
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetPlayerLegacyLevel", universalId, onSuccess, onError);
		}

		public static void GetPlayerBattlepassLevel(string universalId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			PlayerCustomWS.Log.Debug("GetPlayerBattlepassLevel");
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetPlayerBattlepassLevel", universalId, onSuccess, onError);
		}

		public static void GetPlayerTotalLevelByUserId(string universalId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			PlayerCustomWS.Log.Debug("GetPlayerTotalLevelByUserId");
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetPlayerTotalLevelByUserId", universalId, onSuccess, onError);
		}

		public static void IncrementPlayerEndMatchPresenceCounter(string universalId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			PlayerCustomWS.Log.Debug("IncrementPlayerEndMatchPresenceCounter");
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "IncrementPlayerEndMatchPresenceCounter", universalId, onSuccess, onError);
		}

		public static void MarkPlayerHasDoneTutorial(string universalId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			PlayerCustomWS.Log.Debug("MarkPlayerHasDoneTutorial");
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "MarkPlayerHasDoneTutorial", universalId, onSuccess, onError);
		}

		public static void IsPlayerEligibleForRookieQuiz(string universalId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "IsPlayerEligibleForRookieQuiz", universalId, onSuccess, onError);
		}

		public static void DisableTrainingPopUp(string universalId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "DisableTrainingPopUp", universalId, onSuccess, onError);
		}

		public static void UpdatePlayerUnlockMask(PlayerBag bag, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			PlayerCustomWS.Log.DebugFormat("UpdatePlayerUnlockMask {0}", new object[]
			{
				bag
			});
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "UpdatePlayerUnlockMask", bag.ToString(), onSuccess, onError);
		}

		public static void SavePlayerPrefs(string prefs, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			PlayerCustomWS.Log.DebugFormat("SavePlayerPrefs {0}", new object[]
			{
				prefs
			});
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "SavePlayerPrefs", prefs, onSuccess, onError);
		}

		public static void UpdatePlayerNews(PlayerBag bag, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			PlayerCustomWS.Log.DebugFormat("UpdatePlayerNews {0}", new object[]
			{
				bag
			});
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "UpdatePlayerNews", bag.ToString(), onSuccess, onError);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PlayerCustomWS));
	}
}
