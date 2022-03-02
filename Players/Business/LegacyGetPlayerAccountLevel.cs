using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines.Players.Business
{
	public class LegacyGetPlayerAccountLevel : IGetPlayerAccountLevel
	{
		public LegacyGetPlayerAccountLevel(IMatchPlayers matchPlayers, BattlepassConfig battlepassConfig)
		{
			this._matchPlayers = matchPlayers;
			this._battlepassConfig = battlepassConfig;
		}

		public int Get(long playerId)
		{
			PlayerData anyByPlayerId = this._matchPlayers.GetAnyByPlayerId(playerId);
			return anyByPlayerId.Bag.AccountLevel(anyByPlayerId.BattlepassProgress, this._battlepassConfig);
		}

		private readonly IMatchPlayers _matchPlayers;

		private readonly BattlepassConfig _battlepassConfig;
	}
}
