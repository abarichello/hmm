using System;
using System.Linq;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matches;
using UniRx;

namespace HeavyMetalMachines.Infra.Matchmaking.Matches
{
	public class RunningMatchProvider : IRunningMatchProvider
	{
		public RunningMatchProvider(UserInfo userInfo, MatchData matchData, MatchPlayers matchPlayers)
		{
			this._userInfo = userInfo;
			this._matchData = matchData;
			this._matchPlayers = matchPlayers;
		}

		public IObservable<GetRunningMatchResult> GetRunningMatch(string playerId)
		{
			return Observable.Return<GetRunningMatchResult>(this.GetRunningMatchResult());
		}

		private GetRunningMatchResult GetRunningMatchResult()
		{
			PlayerBag bag = this._userInfo.Bag;
			if (this.IsMatchOver())
			{
				return new GetRunningMatchResult
				{
					FoundMatch = false,
					Match = null
				};
			}
			return new GetRunningMatchResult
			{
				FoundMatch = true,
				Match = this.GetMatch(bag)
			};
		}

		private bool IsMatchOver()
		{
			return this._matchData.State == MatchData.MatchState.MatchOverBluWins || this._matchData.State == MatchData.MatchState.MatchOverRedWins || this._matchData.State == MatchData.MatchState.MatchOverTie;
		}

		private Match? GetMatch(PlayerBag playerBag)
		{
			Match value = default(Match);
			value.ArenaIndex = this._matchData.ArenaIndex;
			value.Mode = MatchKindExtensions.ToMatchMode(this._matchData.Kind);
			value.MatchId = playerBag.CurrentMatchId;
			value.Connection = RunningMatchProvider.GetMatchConnection(playerBag);
			value.Clients = this._matchPlayers.Players.Select(delegate(PlayerData client)
			{
				MatchClient result = default(MatchClient);
				result.PlayerId = client.PlayerId;
				return result;
			}).ToArray<MatchClient>();
			return new Match?(value);
		}

		private static MatchConnection GetMatchConnection(PlayerBag playerBag)
		{
			MatchConnection result = default(MatchConnection);
			result.Host = playerBag.CurrentServerIp;
			result.Port = playerBag.CurrentPort;
			return result;
		}

		private readonly UserInfo _userInfo;

		private readonly MatchData _matchData;

		private readonly MatchPlayers _matchPlayers;
	}
}
