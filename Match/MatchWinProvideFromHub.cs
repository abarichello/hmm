using System;
using Pocketverse;

namespace HeavyMetalMachines.Match
{
	public class MatchWinProvideFromHub : GameHubObject, IMatchWinProvider
	{
		public bool MatchWin()
		{
			return (GameHubObject.Hub.Match.State == MatchData.MatchState.MatchOverBluWins && GameHubObject.Hub.Players.CurrentPlayerTeam == TeamKind.Blue) || (GameHubObject.Hub.Match.State == MatchData.MatchState.MatchOverRedWins && GameHubObject.Hub.Players.CurrentPlayerTeam == TeamKind.Red);
		}
	}
}
