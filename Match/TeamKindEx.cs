using System;
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
	}
}
