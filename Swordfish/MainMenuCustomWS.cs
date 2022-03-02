using System;
using ClientAPI;
using HeavyMetalMachines.DataTransferObjects.Player;
using Pocketverse;
using UniRx;

namespace HeavyMetalMachines.Swordfish
{
	public class MainMenuCustomWS : GameHubObject
	{
		public static void GetMainMenuData(object state, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(state, "GetMainMenuData", string.Empty, onSuccess, onError);
		}

		public static IObservable<MainMenuData> GetMainMenuData()
		{
			return SwordfishObservable.FromStringSwordfishCall<MainMenuData>(delegate(SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
			{
				MainMenuCustomWS.GetMainMenuData(null, onSuccess, onError);
			});
		}
	}
}
