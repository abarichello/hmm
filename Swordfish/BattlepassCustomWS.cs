using System;
using System.Collections.Generic;
using ClientAPI;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Items.DataTransferObjects;
using Pocketverse;

namespace HeavyMetalMachines.Swordfish
{
	public class BattlepassCustomWS : GameHubObject
	{
		public static void SelectMissionsForPlayer(long playerID, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			BattlepassCustomWS.Log.DebugFormat("SelectMissionsForPlayer {0}", new object[]
			{
				playerID
			});
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "SelectMissionsForPlayer", playerID.ToString(), onSuccess, onError);
		}

		public static void GiveAccountXP(long playerID, int xpAmount, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GiveAccountXP", string.Format("{0},{1}", playerID, xpAmount), onSuccess, onError);
		}

		public static void UpdateRewards(RewardsBag bag, object state, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(state, "UpdateRewards", bag.ToString(), onSuccess, onError);
		}

		public static void SaveCustomizationsSelected(CustomizationBagAdapter bag, object state, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(state, "SaveCustomizationSelected", bag.ToString(), onSuccess, onError);
		}

		public static void ClaimRewards(object state, long playerId, int levelToClaim, bool premiumClaim, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			ClaimRewardRequest claimRewardRequest = new ClaimRewardRequest
			{
				PlayerId = playerId,
				ClaimIndex = levelToClaim,
				IsPremium = premiumClaim
			};
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(state, "ClaimReward", claimRewardRequest.ToString(), onSuccess, onError);
		}

		public static void ClaimAllRewards(object state, long playerId, List<ClaimRewardInfo> rewardsInfo, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			ClaimAllRewardsRequest claimAllRewardsRequest = new ClaimAllRewardsRequest
			{
				PlayerId = playerId,
				ClaimRewardsInfo = rewardsInfo
			};
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(state, "ClaimAllRewards", claimAllRewardsRequest.ToString(), onSuccess, onError);
		}

		internal static void MarkMissionsAsSeen(object state, string playerId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(state, "MarkMissionsAsSeen", playerId, onSuccess, onError);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BattlepassCustomWS));
	}
}
