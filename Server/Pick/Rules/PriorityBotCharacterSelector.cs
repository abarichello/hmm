using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Server.Pick.Rules.Apis;
using Pocketverse;

namespace HeavyMetalMachines.Server.Pick.Rules
{
	public class PriorityBotCharacterSelector : IBotCharacterSelector
	{
		public PriorityBotCharacterSelector(IItemType[] validCharactersForBots, IMatchPlayers matchPlayers, PriorityBotCharacterSelector.GetBotDesiredPickCb botDesiredPickGetter, IPriorityRolesProvider priorityRolesProvider)
		{
			this._validCharactersForBots = validCharactersForBots;
			this._matchPlayers = matchPlayers;
			this.GetBotDesiredPick = botDesiredPickGetter;
			this._priorityRolesProvider = priorityRolesProvider;
		}

		public int SelectCharacter(PlayerData bot)
		{
			IItemType[] array = this.GetAvailableCharactersForPick(bot);
			if (array.Length <= 0)
			{
				PriorityBotCharacterSelector.Log.WarnFormat("No possible character to randomly select for player={0} team={1}", new object[]
				{
					bot.PlayerAddress,
					bot.Team
				});
				return -1;
			}
			List<DriverRoleKind> priorityRoles = this._priorityRolesProvider.GetPriorityRoles(bot.Team);
			array = PriorityBotCharacterSelector.FilterCharactersByPriorityRoles(array, priorityRoles);
			return PriorityBotCharacterSelector.SelectRandomCharacter(array);
		}

		protected static IItemType[] FilterCharactersByPriorityRoles(IItemType[] availableCharacters, List<DriverRoleKind> priorityRoles)
		{
			IItemType[] array = Array.FindAll<IItemType>(availableCharacters, delegate(IItemType characterInfo)
			{
				CharacterItemTypeComponent component = characterInfo.GetComponent<CharacterItemTypeComponent>();
				return priorityRoles.Contains(component.Role);
			});
			if (array.Length == 0)
			{
				PriorityBotCharacterSelector.Log.WarnFormat("No character available for Bot with the role set in SharedConfig.", new object[0]);
				return availableCharacters;
			}
			return array;
		}

		private static int SelectRandomCharacter(IItemType[] availableCharacters)
		{
			IItemType itemType = availableCharacters[SysRandom.Int(0, availableCharacters.Length)];
			return itemType.GetComponent<CharacterItemTypeComponent>().CharacterId;
		}

		protected IItemType[] GetAvailableCharactersForPick(PlayerData bot)
		{
			return Array.FindAll<IItemType>(this._validCharactersForBots, (IItemType charItemType) => this.IsCharacterAvailableForPick(bot, charItemType));
		}

		private bool IsCharacterAvailableForPick(PlayerData bot, IItemType charItemType)
		{
			CharacterItemTypeComponent component = charItemType.GetComponent<CharacterItemTypeComponent>();
			return !this.IsCharacterReservedInTeam(bot.PlayerAddress, bot.Team, component.CharacterId);
		}

		private bool IsCharacterReservedInTeam(byte targetAddress, TeamKind team, int characterId)
		{
			List<PlayerData> playersAndBotsByTeam = this._matchPlayers.GetPlayersAndBotsByTeam(team);
			if (playersAndBotsByTeam == null)
			{
				PriorityBotCharacterSelector.Log.ErrorFormat("Player={0} asking picked or reserved with invalid team={1}", new object[]
				{
					targetAddress,
					team
				});
				return true;
			}
			for (int i = 0; i < playersAndBotsByTeam.Count; i++)
			{
				PlayerData playerData = playersAndBotsByTeam[i];
				if (playerData.PlayerAddress != targetAddress)
				{
					if (this.IsCharacterPickedByPlayer(playerData, characterId) || this.IsCharacterDesiredByBot(playerData, characterId))
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool IsCharacterPickedByPlayer(PlayerData player, int characterId)
		{
			return player.CharacterId == characterId;
		}

		private bool IsCharacterDesiredByBot(PlayerData player, int characterId)
		{
			return player.IsBot && this.GetBotDesiredPick(player.PlayerAddress) == characterId;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PriorityBotCharacterSelector));

		private readonly IItemType[] _validCharactersForBots;

		private readonly IMatchPlayers _matchPlayers;

		private readonly IPriorityRolesProvider _priorityRolesProvider;

		public readonly PriorityBotCharacterSelector.GetBotDesiredPickCb GetBotDesiredPick;

		public delegate int GetBotDesiredPickCb(byte playerAddress);
	}
}
