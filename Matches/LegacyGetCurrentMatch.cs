using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Match.Infra;

namespace HeavyMetalMachines.Matches
{
	public class LegacyGetCurrentMatch : IGetCurrentMatch
	{
		public LegacyGetCurrentMatch(IMatchPlayersProvider matchPlayersProvider, HMMHub hub)
		{
			this._matchPlayersProvider = matchPlayersProvider;
			this._hub = hub;
		}

		public Match? GetIfExisting()
		{
			if (this._hub.Match.State == MatchData.MatchState.Nothing)
			{
				return null;
			}
			List<PlayerData> playersAndBotsByTeam = this._matchPlayersProvider.GetMatchPlayers.GetPlayersAndBotsByTeam(TeamKind.Blue);
			Match value = default(Match);
			value.ArenaIndex = this._hub.Match.ArenaIndex;
			value.Clients = this.GetClients();
			value.Mode = MatchKindExtensions.ToMatchMode(this._hub.Match.Kind);
			value.PlayerCountPerTeam = (short)playersAndBotsByTeam.Count;
			value.MatchId = ((!this._hub.Net.IsServer()) ? this._hub.Swordfish.Msg.ClientMatchId.ToString() : this._hub.Swordfish.Connection.ServerMatchId.ToString());
			return new Match?(value);
		}

		private MatchClient[] GetClients()
		{
			IMatchPlayers getMatchPlayers = this._matchPlayersProvider.GetMatchPlayers;
			IEnumerable<PlayerData> playersAndBotsByTeam = getMatchPlayers.GetPlayersAndBotsByTeam(TeamKind.Blue);
			if (LegacyGetCurrentMatch.<>f__mg$cache0 == null)
			{
				LegacyGetCurrentMatch.<>f__mg$cache0 = new Func<PlayerData, MatchClient>(PlayerDataExtension.ToMatchClient);
			}
			IEnumerable<MatchClient> first = playersAndBotsByTeam.Select(LegacyGetCurrentMatch.<>f__mg$cache0);
			IEnumerable<PlayerData> playersAndBotsByTeam2 = getMatchPlayers.GetPlayersAndBotsByTeam(TeamKind.Red);
			if (LegacyGetCurrentMatch.<>f__mg$cache1 == null)
			{
				LegacyGetCurrentMatch.<>f__mg$cache1 = new Func<PlayerData, MatchClient>(PlayerDataExtension.ToMatchClient);
			}
			IEnumerable<MatchClient> second = playersAndBotsByTeam2.Select(LegacyGetCurrentMatch.<>f__mg$cache1);
			IEnumerable<PlayerData> narrators = getMatchPlayers.Narrators;
			if (LegacyGetCurrentMatch.<>f__mg$cache2 == null)
			{
				LegacyGetCurrentMatch.<>f__mg$cache2 = new Func<PlayerData, MatchClient>(PlayerDataExtension.ToMatchClient);
			}
			IEnumerable<MatchClient> second2 = narrators.Select(LegacyGetCurrentMatch.<>f__mg$cache2);
			return first.Concat(second).Concat(second2).ToArray<MatchClient>();
		}

		private readonly IMatchPlayersProvider _matchPlayersProvider;

		private readonly HMMHub _hub;

		[CompilerGenerated]
		private static Func<PlayerData, MatchClient> <>f__mg$cache0;

		[CompilerGenerated]
		private static Func<PlayerData, MatchClient> <>f__mg$cache1;

		[CompilerGenerated]
		private static Func<PlayerData, MatchClient> <>f__mg$cache2;
	}
}
