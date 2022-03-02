using System;
using System.Collections.Generic;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Server.Pick.Rules.Apis;

namespace HeavyMetalMachines.Server.Pick.Rules
{
	public class PriorityRolesProvider : IPriorityRolesProvider
	{
		public PriorityRolesProvider(IMatchPlayers matchPlayers, DriverRoleKind[] teamRoles, PriorityRolesProvider.GetBotDesiredPickCb botDesiredPickGetter, PriorityRolesProvider.GetDriverRoleKindByCharIdCb driverRoleKindByCharIdGetter)
		{
			this._matchPlayers = matchPlayers;
			this._teamRoles = teamRoles;
			this.GetBotDesiredPick = botDesiredPickGetter;
			this.GetDriverRoleKindByCharId = driverRoleKindByCharIdGetter;
		}

		public List<DriverRoleKind> GetPriorityRoles(TeamKind team)
		{
			List<DriverRoleKind> list = new List<DriverRoleKind>(this._teamRoles);
			List<PlayerData> playersAndBotsByTeam = this._matchPlayers.GetPlayersAndBotsByTeam(team);
			for (int i = 0; i < playersAndBotsByTeam.Count; i++)
			{
				PlayerData playerData = playersAndBotsByTeam[i];
				if (PriorityRolesProvider.IsPlayerCharacterDefined(playerData))
				{
					list.Remove(playerData.GetCharacterRole());
				}
				if (playerData.IsBot && this.IsBotDesiredPickDefined(playerData))
				{
					int characterId = this.GetBotDesiredPick(playerData.PlayerAddress);
					DriverRoleKind item = this.GetDriverRoleKindByCharId(characterId);
					list.Remove(item);
				}
			}
			return list;
		}

		private static bool IsPlayerCharacterDefined(PlayerData player)
		{
			return player.CharacterItemType != null;
		}

		private bool IsBotDesiredPickDefined(PlayerData bot)
		{
			int num = this.GetBotDesiredPick(bot.PlayerAddress);
			return num != -1;
		}

		private readonly IMatchPlayers _matchPlayers;

		private readonly DriverRoleKind[] _teamRoles;

		private readonly PriorityRolesProvider.GetBotDesiredPickCb GetBotDesiredPick;

		private readonly PriorityRolesProvider.GetDriverRoleKindByCharIdCb GetDriverRoleKindByCharId;

		public delegate int GetBotDesiredPickCb(byte playerAddress);

		public delegate DriverRoleKind GetDriverRoleKindByCharIdCb(int characterId);
	}
}
