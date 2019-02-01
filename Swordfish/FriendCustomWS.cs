using System;
using ClientAPI;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Swordfish
{
	public class FriendCustomWS : GameHubObject
	{
		public static void GetUserFriendByPlayerId(long playerId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			Debug.Log("XXX REMOVE THIS CALL!!!! NEW SF API!!!");
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetUserFriendByPlayerId", playerId.ToString(), onSuccess, onError);
		}

		public static void GetUserFriendByUserId(Guid userId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			Debug.Log("XXX REMOVE THIS CALL!!!! NEW SF API!!!");
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetUserFriendByUserId", userId.ToString(), onSuccess, onError);
		}
	}
}
