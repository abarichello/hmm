using System;
using System.Collections;
using ClientAPI;
using ClientAPI.Objects;
using HeavyMetalMachines.DataTransferObjects.Server;
using Pocketverse;
using Swordfish.Common.exceptions;
using UnityEngine;

namespace HeavyMetalMachines.Swordfish
{
	public class StorytellerCustomWS : GameHubObject
	{
		public static void GetMatches(object state, MatchSearchBag search, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			if (GameHubObject.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				GameHubObject.Hub.StartCoroutine(StorytellerCustomWS.FakeGetMatches(state, search, onSuccess, onError));
				return;
			}
			GameHubObject.Hub.ClientApi.customws.ExecuteCustomWSWithReturn(state, "GetMatches", (string)search, onSuccess, onError);
		}

		private static IEnumerator FakeGetMatches(object state, MatchSearchBag search, SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
		{
			yield return new WaitForSeconds(1f);
			int bagsize = 12;
			if (search.SearchFilter.Contains("size"))
			{
				string[] array = search.SearchFilter.Split(new char[]
				{
					'='
				});
				if (array.Length == 2)
				{
					int.TryParse(array[1], out bagsize);
				}
			}
			if (search.SearchFilter.Contains("error"))
			{
				onError.Invoke(null, new CustomWSException("FakeCustomWS error!"));
				yield break;
			}
			GameServerInfoListBag bag = new GameServerInfoListBag
			{
				Servers = new GameServerRunningInfo[bagsize]
			};
			for (int i = 0; i < bag.Servers.Length; i++)
			{
				Guid matchId = Guid.NewGuid();
				bag.Servers[i] = new GameServerRunningInfo
				{
					Ip = GameHubObject.Hub.Config.GetValue(ConfigAccess.ServerIP),
					Port = new int?(GameHubObject.Hub.Config.GetIntValue(ConfigAccess.ServerPort)),
					GameServerStatus = new ServerStatusBag
					{
						BluTeam = new string[]
						{
							"-1",
							"-1",
							"-1",
							"-1"
						},
						RedTeam = new string[]
						{
							"-1",
							"-1",
							"-1",
							"-1"
						},
						BluTeamName = ((i % 3 != 0) ? ("Blus" + i) : string.Empty),
						RedTeamName = ((i % 2 != 0) ? ("Reds" + i) : string.Empty),
						BluTeamIconURL = ((i % 7 != 0) ? ("team_image_meme_0" + i % 6) : null),
						RedTeamIconURL = ((i % 5 != 0) ? "team_image_meme_02" : null),
						BluTeamPlayerNames = new string[]
						{
							"a",
							"b",
							"c",
							"d"
						},
						RedTeamPlayerNames = new string[]
						{
							"e",
							"f",
							"g",
							"h"
						},
						BluTeamPlayerTags = new long[]
						{
							1L,
							2L,
							3L,
							4L,
							5L
						},
						RedTeamPlayerTags = new long[]
						{
							6L,
							7L,
							8L,
							9L,
							10L
						},
						RedTeamPublisherIds = new int[]
						{
							0,
							1,
							1,
							0,
							2
						},
						BluTeamPublisherIds = new int[]
						{
							1,
							0,
							2,
							0,
							2
						},
						RedTeamPublisherUserNames = new string[]
						{
							"@",
							"@",
							"@",
							"@"
						},
						BluTeamPublisherUserNames = new string[]
						{
							"@",
							"@",
							"@",
							"@"
						},
						Date = (DateTime.UtcNow - TimeSpan.FromMinutes((double)(1 + i * 20))).ToString("yyyy-MM-dd HH:mm:ss"),
						MatchId = matchId,
						ServerPhase = (i + 2) % 4,
						StorytellerCount = i % 3
					}.ToString(),
					MatchId = matchId
				};
			}
			onSuccess.Invoke(state, bag.ToString());
			yield break;
		}
	}
}
