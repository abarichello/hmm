using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ClientAPI;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Logs;
using Pocketverse;

namespace HeavyMetalMachines.Match
{
	public static class EndMatchPresenceUpdater
	{
		public static void UpdateEndMatchPresence(List<PlayerEndGamePresence.PresenceData> playerPresenceList)
		{
			for (int i = 0; i < playerPresenceList.Count; i++)
			{
				PlayerEndGamePresence.PresenceData presenceData = playerPresenceList[i];
				if (presenceData.Present)
				{
					EndMatchPresenceUpdater.UpdateEndMatchPresenceForPlayer(presenceData.PlayerId);
				}
			}
		}

		private static void UpdateEndMatchPresenceForPlayer(string playerId)
		{
			if (EndMatchPresenceUpdater.<>f__mg$cache0 == null)
			{
				EndMatchPresenceUpdater.<>f__mg$cache0 = new SwordfishClientApi.ParameterizedCallback<string>(EndMatchPresenceUpdater.OnSuccess);
			}
			SwordfishClientApi.ParameterizedCallback<string> onSuccess = EndMatchPresenceUpdater.<>f__mg$cache0;
			if (EndMatchPresenceUpdater.<>f__mg$cache1 == null)
			{
				EndMatchPresenceUpdater.<>f__mg$cache1 = new SwordfishClientApi.ErrorCallback(EndMatchPresenceUpdater.OnError);
			}
			PlayerCustomWS.IncrementPlayerEndMatchPresenceCounter(playerId, onSuccess, EndMatchPresenceUpdater.<>f__mg$cache1);
		}

		private static void OnSuccess(object state, string obj)
		{
			NetResult netResult = (NetResult)((JsonSerializeable<T>)obj);
			if (netResult.Success)
			{
				EndMatchPresenceUpdater.Log.InfoFormat("Successfully updated player end match presence counter.", new object[0]);
			}
			else
			{
				EndMatchPresenceUpdater.Log.ErrorFormat("Failed to update player end match presence counter, error={0} msg={1}", new object[]
				{
					netResult.Error,
					netResult.Msg
				});
			}
		}

		private static void OnError(object state, Exception exception)
		{
			EndMatchPresenceUpdater.Log.ErrorFormat("Error while updating player end match presence counter. exception: ", new object[]
			{
				exception
			});
		}

		public static readonly BitLogger Log = new BitLogger(typeof(EndMatchPresenceUpdater));

		[CompilerGenerated]
		private static SwordfishClientApi.ParameterizedCallback<string> <>f__mg$cache0;

		[CompilerGenerated]
		private static SwordfishClientApi.ErrorCallback <>f__mg$cache1;
	}
}
