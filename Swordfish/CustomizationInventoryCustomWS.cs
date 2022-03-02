using System;
using ClientAPI;
using HeavyMetalMachines.DataTransferObjects.Progression;
using Pocketverse;

namespace HeavyMetalMachines.Swordfish
{
	public class CustomizationInventoryCustomWS : GameHubObject
	{
		public static void MarkItemAsSeen(object state, long itemId, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			CustomizationInventoryCustomWS.MarkItemAsSeen(state, new long[]
			{
				itemId
			}, onSuccess, onError);
		}

		public static void MarkItemAsSeen(object state, long[] itemsIds, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			MarkAsSeenBag markAsSeenBag = new MarkAsSeenBag();
			markAsSeenBag.ItensIds = itemsIds;
			markAsSeenBag.UserId = GameHubObject.Hub.User.UserSF.Id;
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(state, "MarkAsSeenMultipleItens", markAsSeenBag.ToString(), onSuccess, onError);
		}
	}
}
