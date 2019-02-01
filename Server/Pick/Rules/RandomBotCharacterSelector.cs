using System;
using System.Collections.Generic;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Server.Pick.Rules.Apis;
using UnityEngine;

namespace HeavyMetalMachines.Server.Pick.Rules
{
	public class RandomBotCharacterSelector : IBotCharacterSelector
	{
		public RandomBotCharacterSelector(HeavyMetalMachines.Character.CharacterInfo[] validCharactersForBots, List<PlayerData> players, List<PlayerData> bots)
		{
			this._validCharactersForBots = validCharactersForBots;
			this._players = players;
			this._bots = bots;
		}

		public int SelectCharacter(PlayerData bot)
		{
			int[] availableCharactersForBotSelection = this.GetAvailableCharactersForBotSelection(bot);
			if (availableCharactersForBotSelection.Length <= 0)
			{
				return -1;
			}
			return availableCharactersForBotSelection[UnityEngine.Random.Range(0, availableCharactersForBotSelection.Length)];
		}

		protected int[] GetAvailableCharactersForBotSelection(PlayerData bot)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < this._validCharactersForBots.Length; i++)
			{
				HeavyMetalMachines.Character.CharacterInfo characterInfo = this._validCharactersForBots[i];
				if (this.IsCharacterAvailableForSelection(bot.PlayerAddress, bot.Team, characterInfo.CharacterId))
				{
					list.Add(characterInfo.CharacterId);
				}
			}
			return list.ToArray();
		}

		private bool IsCharacterAvailableForSelection(byte botAddress, TeamKind botTeam, int characterId)
		{
			return !this.IsCharacterUnavailableForSelection(botAddress, botTeam, characterId);
		}

		private bool IsCharacterUnavailableForSelection(byte botAddress, TeamKind botTeam, int characterId)
		{
			return this.IsCharacterSelectedByBot(botAddress, botTeam, characterId) || this.IsCharacterPickedByPlayer(botTeam, characterId);
		}

		private bool IsCharacterSelectedByBot(byte botAddress, TeamKind botTeam, int characterId)
		{
			for (int i = 0; i < this._bots.Count; i++)
			{
				PlayerData playerData = this._bots[i];
				if (playerData.PlayerAddress != botAddress && !RandomBotCharacterSelector.AreDifferentTeams(playerData.Team, botTeam))
				{
					if (playerData.SelectedChar == characterId)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool IsCharacterPickedByPlayer(TeamKind botTeam, int botCharacterId)
		{
			for (int i = 0; i < this._players.Count; i++)
			{
				PlayerData playerData = this._players[i];
				if (!RandomBotCharacterSelector.AreDifferentTeams(playerData.Team, botTeam))
				{
					if (playerData.CharacterId == botCharacterId)
					{
						return true;
					}
				}
			}
			return false;
		}

		private static bool AreDifferentTeams(TeamKind teamA, TeamKind teamB)
		{
			return teamA != teamB && teamB != TeamKind.Zero;
		}

		private readonly HeavyMetalMachines.Character.CharacterInfo[] _validCharactersForBots;

		private readonly List<PlayerData> _players;

		private readonly List<PlayerData> _bots;
	}
}
