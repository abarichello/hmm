using System;

namespace HeavyMetalMachines.Match
{
	public static class MatchStateEx
	{
		public static bool IsGame(this MatchData.MatchState state)
		{
			return state == MatchData.MatchState.PreMatch || state == MatchData.MatchState.MatchStarted || state == MatchData.MatchState.Tutorial;
		}

		public static bool IsRunning(this MatchData.MatchState state)
		{
			switch (state)
			{
			case MatchData.MatchState.MatchOverRedWins:
			case MatchData.MatchState.MatchOverBluWins:
			case MatchData.MatchState.MatchOverTie:
				return false;
			default:
				return true;
			}
		}

		public static bool IsOver(this MatchData.MatchState state)
		{
			switch (state)
			{
			case MatchData.MatchState.MatchOverRedWins:
			case MatchData.MatchState.MatchOverBluWins:
			case MatchData.MatchState.MatchOverTie:
				return true;
			default:
				return false;
			}
		}
	}
}
