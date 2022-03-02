using System;
using HeavyMetalMachines.Matches;
using Pocketverse;

namespace HeavyMetalMachines.Match
{
	public static class TeamKindEx
	{
		public static void WriteTeamKind(this BitStream stream, TeamKind kind)
		{
			stream.WriteBits(2, (int)kind);
		}

		public static TeamKind ReadTeamKind(this BitStream stream)
		{
			return (TeamKind)stream.ReadBits(2);
		}

		public static TeamKind GetEnemyTeam(this TeamKind teamKind)
		{
			switch (teamKind)
			{
			case TeamKind.Zero:
			case TeamKind.Neutral:
				return teamKind;
			case TeamKind.Red:
				return TeamKind.Blue;
			case TeamKind.Blue:
				return TeamKind.Red;
			default:
				return TeamKind.Zero;
			}
		}

		public static MatchTeam GetMatchTeam(this TeamKind teamKind)
		{
			if (teamKind == TeamKind.Neutral)
			{
				return 2;
			}
			if (teamKind == TeamKind.Blue)
			{
				return 0;
			}
			if (teamKind != TeamKind.Red)
			{
				throw new Exception(string.Format("Cannot convert team kind {0} into a MatchTeam.", teamKind));
			}
			return 1;
		}
	}
}
