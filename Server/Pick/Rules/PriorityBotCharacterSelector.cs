using System;
using System.Collections.Generic;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Server.Pick.Rules.Apis;
using Pocketverse;

namespace HeavyMetalMachines.Server.Pick.Rules
{
	public class PriorityBotCharacterSelector : IBotCharacterSelector
	{
		public PriorityBotCharacterSelector(CharacterInfo[] validCharactersForBots, IMatchPlayers matchPlayers, PriorityBotCharacterSelector.GetBotDesiredPickCb botDesiredPickGetter, IPriorityRolesProvider priorityRolesProvider)
		{
			this._validCharactersForBots = validCharactersForBots;
			this._matchPlayers = matchPlayers;
			this.GetBotDesiredPick = botDesiredPickGetter;
			this._priorityRolesProvider = priorityRolesProvider;
		}

		public int SelectCharacter(PlayerData bot)
		{
			CharacterInfo[] array = this.GetAvailableCharactersForPick(bot);
			if (array.Length <= 0)
			{
				PriorityBotCharacterSelector.Log.WarnFormat("No possible character to randomly select for player={0} team={1}", new object[]
				{
					bot.PlayerAddress,
					bot.Team
				});
				return -1;
			}
			List<CharacterInfo.DriverRoleKind> priorityRoles = this._priorityRolesProvider.GetPriorityRoles(bot.Team);
			array = PriorityBotCharacterSelector.FilterCharactersByPriorityRoles(array, priorityRoles);
			return PriorityBotCharacterSelector.SelectRandomCharacter(array);
		}

		protected static CharacterInfo[] FilterCharactersByPriorityRoles(CharacterInfo[] availableCharacters, List<CharacterInfo.DriverRoleKind> priorityRoles)
		{
			CharacterInfo[] array = Array.FindAll<CharacterInfo>(availableCharacters, (CharacterInfo characterInfo) => priorityRoles.Contains(characterInfo.Role));
			if (array.Length == 0)
			{
				PriorityBotCharacterSelector.Log.WarnFormat("No character available for Bot with the role set in SharedConfig.", new object[0]);
				return availableCharacters;
			}
			return array;
		}

		private static int SelectRandomCharacter(IList<CharacterInfo> availableCharacters)
		{
			return availableCharacters[SysRandom.Int(0, availableCharacters.Count)].CharacterId;
		}

		protected CharacterInfo[] GetAvailableCharactersForPick(PlayerData bot)
		{
			return Array.FindAll<CharacterInfo>(this._validCharactersForBots, (CharacterInfo characterInfo) => this.IsCharacterAvailableForPick(bot.PlayerAddress, bot.Team, characterInfo.CharacterId));
		}

		private bool IsCharacterAvailableForPick(byte targetAddress, TeamKind team, int characterId)
		{
			return !this.IsCharacterReservedInTeam(targetAddress, team, characterId);
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

		private readonly CharacterInfo[] _validCharactersForBots;

		private readonly IMatchPlayers _matchPlayers;

		private readonly IPriorityRolesProvider _priorityRolesProvider;

		public PriorityBotCharacterSelector.GetBotDesiredPickCb GetBotDesiredPick;

		public delegate int GetBotDesiredPickCb(byte playerAddress);
	}
}
