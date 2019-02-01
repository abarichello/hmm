using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class PlayerColorsInfo : GameHubScriptableObject
	{
		public Color GetColorForPlayerAddressAndTeam(TeamKind team, int teamslot)
		{
			if (team != TeamKind.Red)
			{
				if (team != TeamKind.Blue)
				{
				}
				return this.BlueTeamColors[teamslot];
			}
			return this.RedTeamColors[teamslot];
		}

		public Color[] PlayerColors;

		public Color[] RedTeamColors;

		public Color[] BlueTeamColors;
	}
}
