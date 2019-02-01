using System;
using ClientAPI;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;

namespace HeavyMetalMachines.Swordfish
{
	public class CharacterCustomWS : GameHubObject
	{
		public static void ServerEquipCharacter(CharacterBag bag, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "ServerEquipCharacter", bag.ToString(), onSuccess, onError);
		}

		public static void ServerUpdateCharacterStats(CharacterBag bag, object state, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(state, "ServerUpdateCharacterStats", bag.ToString(), onSuccess, onError);
		}

		public static void EquipCharacter(CharacterBag bag, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "EquipCharacter", bag.ToString(), onSuccess, onError);
		}

		public static void UpdateCharacterUnlockMask(CharacterBag bag, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "UpdateCharacterUnlockMask", bag.ToString(), onSuccess, onError);
		}

		public static void GetRotation(SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(null, "GetRotation", string.Empty, onSuccess, onError);
		}
	}
}
