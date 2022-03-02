using System;
using System.Collections.Generic;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines
{
	public interface IMatchPlayers
	{
		List<PlayerData> GetPlayersAndBotsByTeam(TeamKind team);

		List<PlayerData> PlayersAndBots { get; }

		List<PlayerData> Players { get; }

		List<PlayerData> Bots { get; }

		List<PlayerData> Narrators { get; }

		PlayerData GetPlayerByObjectId(int playerId);

		PlayerData GetPlayerByAddress(byte address);

		PlayerData GetPlayerOrBotsByObjectId(int playerId);

		PlayerData GetAnyByPlayerId(long playerId);

		PlayerData CurrentPlayerData { get; }

		TeamKind GetTeamKindById(int playerId);

		int RedMMR { get; }

		int BlueMMR { get; }

		bool IsTeamBotOnly(List<PlayerData> playerDatas);

		void UpdatePlayers();
	}
}
