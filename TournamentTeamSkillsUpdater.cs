using System;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Tournaments.DataTransferObjects;
using Hoplon.Serialization;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class TournamentTeamSkillsUpdater
	{
		public IFuture UpdateTeamSkills(string redTeamMemberUniversalId, string blueTeamMemberUniversalId, TeamKind winner, long tournamentStepId, TeamStatsDTO redTeamStats, TeamStatsDTO blueTeamStats)
		{
			TeamTournamentSkillsDTO teamTournamentSkillsDTO = new TeamTournamentSkillsDTO
			{
				team1MemberUniversalId = redTeamMemberUniversalId,
				team2MemberUniversalId = blueTeamMemberUniversalId,
				winnerTeam = ((winner != TeamKind.Red) ? 2 : 1),
				tournamentStepId = tournamentStepId,
				team1Stats = redTeamStats,
				team2Stats = blueTeamStats
			};
			Future future = new Future
			{
				Name = "updateTeamSkills"
			};
			TournamentTeamSkillsUpdater.TeamSkillsUpdateStateObject state = new TournamentTeamSkillsUpdater.TeamSkillsUpdateStateObject
			{
				PendingFuture = future
			};
			TeamCustomWS.UpdateTournamentTeamsSkills(teamTournamentSkillsDTO, delegate(object o, string s)
			{
				NetResult netResult = (NetResult)((JsonSerializeable<!0>)s);
				if (netResult.Success)
				{
					TournamentTeamSkillsUpdater.Log.DebugFormat("UpdateTournamentTeamsSkills Success. team1Member: {0}, team2Member: {1}", new object[]
					{
						redTeamMemberUniversalId,
						blueTeamMemberUniversalId
					});
				}
				else
				{
					TournamentTeamSkillsUpdater.Log.ErrorFormat("UpdateTournamentTeamsSkills Failed. team1Member: {0}, team2Member: {1} Error = {2}", new object[]
					{
						redTeamMemberUniversalId,
						blueTeamMemberUniversalId,
						netResult.Msg
					});
				}
				state.PendingFuture.Result = 0;
			}, delegate(object o, Exception exception)
			{
				TournamentTeamSkillsUpdater.Log.ErrorFormat("Error on UpdateTournamentTeamsSkills. Exception: {0}", new object[]
				{
					exception
				});
				state.PendingFuture.Result = 0;
			});
			return future;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(TournamentTeamSkillsUpdater));

		private struct TeamSkillsUpdateStateObject
		{
			public IFuture PendingFuture;
		}
	}
}
