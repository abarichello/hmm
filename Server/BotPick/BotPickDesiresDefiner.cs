using System;
using System.Collections.Generic;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Server.BotPick
{
	public class BotPickDesiresDefiner
	{
		public BotPickDesiresDefiner(IMatchPlayers matchPlayers, BotPickDesiresDefiner.SelectPriorityCharacterForBotCb selectPriorityCharacterForBot, IConfigLoader configLoader, Dictionary<byte, BotPickData> botsPickData)
		{
			this._matchPlayers = matchPlayers;
			this.SelectPriorityCharacterForBot = selectPriorityCharacterForBot;
		}

		public void DefineBotsDesires()
		{
			List<PlayerData> list = new List<PlayerData>();
			List<PlayerData> list2 = new List<PlayerData>();
			this.FilterBotsByTeam(list, list2);
			while (list2.Count > 0 || list.Count > 0)
			{
				PlayerData playerData = BotPickDesiresDefiner.RetrieveRandomBotFromTeams(list, list2);
				playerData.SelectedChar = -1;
				BotPickDesiresDefiner.Log.DebugFormat("Defining bot={0} desire from team={1}", new object[]
				{
					playerData.PlayerAddress,
					playerData.Team
				});
				this.SelectPriorityCharacterForBot(playerData);
			}
			BotPickDesiresDefiner.Log.Debug("Bot Desires End");
		}

		private void FilterBotsByTeam(ICollection<PlayerData> blueBots, ICollection<PlayerData> redBots)
		{
			for (int i = 0; i < this._matchPlayers.Bots.Count; i++)
			{
				PlayerData playerData = this._matchPlayers.Bots[i];
				if (playerData.Team == TeamKind.Blue)
				{
					blueBots.Add(playerData);
				}
				else if (playerData.Team == TeamKind.Red)
				{
					redBots.Add(playerData);
				}
				else
				{
					BotPickDesiresDefiner.Log.ErrorFormat("Invalid team {0} for bot {1} team", new object[]
					{
						playerData.Team,
						playerData.PlayerAddress
					});
				}
			}
		}

		private static PlayerData RetrieveRandomBotFromTeams(IList<PlayerData> blueBots, IList<PlayerData> redBots)
		{
			int num = redBots.Count + blueBots.Count;
			if (Random.Range(0, num) < redBots.Count)
			{
				return BotPickDesiresDefiner.RetrieveRandomBotFromTeam(redBots);
			}
			return BotPickDesiresDefiner.RetrieveRandomBotFromTeam(blueBots);
		}

		private static PlayerData RetrieveRandomBotFromTeam(IList<PlayerData> bots)
		{
			int index = Random.Range(0, bots.Count);
			PlayerData result = bots[index];
			bots.RemoveAt(index);
			return result;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BotPickDesiresDefiner));

		private readonly IMatchPlayers _matchPlayers;

		private readonly BotPickDesiresDefiner.SelectPriorityCharacterForBotCb SelectPriorityCharacterForBot;

		public delegate void SelectPriorityCharacterForBotCb(PlayerData bot);
	}
}
