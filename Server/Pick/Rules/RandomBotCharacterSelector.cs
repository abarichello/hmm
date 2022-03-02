using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Server.Pick.Rules.Apis;
using UnityEngine;

namespace HeavyMetalMachines.Server.Pick.Rules
{
	public class RandomBotCharacterSelector : IBotCharacterSelector
	{
		public RandomBotCharacterSelector(IItemType[] validCharactersForBots, List<PlayerData> players, List<PlayerData> bots)
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
			return availableCharactersForBotSelection[Random.Range(0, availableCharactersForBotSelection.Length)];
		}

		protected int[] GetAvailableCharactersForBotSelection(PlayerData bot)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < this._validCharactersForBots.Length; i++)
			{
				IItemType itemType = this._validCharactersForBots[i];
				if (this.IsCharacterAvailableForSelection(bot, itemType))
				{
					list.Add(itemType.GetComponent<CharacterItemTypeComponent>().CharacterId);
				}
			}
			return list.ToArray();
		}

		private bool IsCharacterAvailableForSelection(PlayerData bot, IItemType charItemType)
		{
			CharacterItemTypeComponent component = charItemType.GetComponent<CharacterItemTypeComponent>();
			return !this.IsCharacterUnavailableForSelection(bot.PlayerAddress, bot.Team, component.CharacterId);
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

		private readonly IItemType[] _validCharactersForBots;

		private readonly List<PlayerData> _players;

		private readonly List<PlayerData> _bots;
	}
}
