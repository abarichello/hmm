using System;
using System.Collections.Generic;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Server.Pick.Rules.Apis;

namespace HeavyMetalMachines.Server.Pick.Rules
{
	public class PriorityRolesProvider : IPriorityRolesProvider
	{
		public PriorityRolesProvider(IMatchPlayers matchPlayers, CharacterInfo.DriverRoleKind[] teamRoles, PriorityRolesProvider.GetBotDesiredPickCb botDesiredPickGetter, PriorityRolesProvider.GetDriverRoleKindByCharIdCb driverRoleKindByCharIdGetter)
		{
			this._matchPlayers = matchPlayers;
			this._teamRoles = teamRoles;
			this.GetBotDesiredPick = botDesiredPickGetter;
			this.GetDriverRoleKindByCharId = driverRoleKindByCharIdGetter;
		}

		public List<CharacterInfo.DriverRoleKind> GetPriorityRoles(TeamKind team)
		{
			List<CharacterInfo.DriverRoleKind> list = new List<CharacterInfo.DriverRoleKind>(this._teamRoles);
			List<PlayerData> playersAndBotsByTeam = this._matchPlayers.GetPlayersAndBotsByTeam(team);
			for (int i = 0; i < playersAndBotsByTeam.Count; i++)
			{
				PlayerData playerData = playersAndBotsByTeam[i];
				if (PriorityRolesProvider.IsPlayerCharacterDefined(playerData))
				{
					list.Remove(playerData.Character.Role);
				}
				if (playerData.IsBot && this.IsBotDesiredPickDefined(playerData))
				{
					int characterId = this.GetBotDesiredPick(playerData.PlayerAddress);
					CharacterInfo.DriverRoleKind item = this.GetDriverRoleKindByCharId(characterId);
					list.Remove(item);
				}
			}
			return list;
		}

		private static bool IsPlayerCharacterDefined(PlayerData player)
		{
			return player.Character != null;
		}

		private bool IsBotDesiredPickDefined(PlayerData bot)
		{
			int num = this.GetBotDesiredPick(bot.PlayerAddress);
			return num != -1;
		}

		private readonly IMatchPlayers _matchPlayers;

		private readonly CharacterInfo.DriverRoleKind[] _teamRoles;

		private readonly PriorityRolesProvider.GetBotDesiredPickCb GetBotDesiredPick;

		private readonly PriorityRolesProvider.GetDriverRoleKindByCharIdCb GetDriverRoleKindByCharId;

		public delegate int GetBotDesiredPickCb(byte playerAddress);

		public delegate CharacterInfo.DriverRoleKind GetDriverRoleKindByCharIdCb(int characterId);
	}
}
