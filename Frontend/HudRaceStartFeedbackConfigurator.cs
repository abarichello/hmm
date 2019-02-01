using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudRaceStartFeedbackConfigurator : GameHubBehaviour
	{
		public void Awake()
		{
			this.SetupRaceStartFeedbackOffsetByTeam(GameHubBehaviour.Hub.Players.CurrentPlayerTeam);
		}

		private void SetupRaceStartFeedbackOffsetByTeam(TeamKind team)
		{
			GameArenaInfo currentArena = GameHubBehaviour.Hub.ArenaConfig.GetCurrentArena();
			float num = 0f;
			if (team == TeamKind.Blue)
			{
				num = currentArena.BlueTeamRaceStartFeedbackHorOffset;
			}
			else if (team == TeamKind.Red)
			{
				num = currentArena.RedTeamRaceStartFeedbackHorOffset;
			}
			Vector3 position = base.transform.position;
			position.x += num;
			base.transform.localPosition = position;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HudRaceStartFeedbackConfigurator));
	}
}
