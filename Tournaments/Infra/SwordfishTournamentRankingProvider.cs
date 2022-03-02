using System;
using System.Runtime.CompilerServices;
using HeavyMetalMachines.DataTransferObjects.Tournament;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Tournaments.DataTransferObjects;
using HeavyMetalMachines.Tournaments.Infra.Exceptions;
using HeavyMetalMachines.Tournaments.Ranking;
using UniRx;

namespace HeavyMetalMachines.Tournaments.Infra
{
	public class SwordfishTournamentRankingProvider : ITournamentRankingProvider
	{
		public IObservable<TournamentRanking> GetGeneralRanking(long tournamentId, int numberOfPositions)
		{
			IObservable<TournamentRanking> tournamentRankByTournamentIdDetailed = TournamentCustomWS.GetTournamentRankByTournamentIdDetailed(new TournamentRankingParameters
			{
				TournamentId = tournamentId,
				RankAmount = (long)numberOfPositions
			});
			if (SwordfishTournamentRankingProvider.<>f__mg$cache0 == null)
			{
				SwordfishTournamentRankingProvider.<>f__mg$cache0 = new Func<TournamentRanking, TournamentRanking>(SwordfishTournamentRankingProvider.ConvertToTournamentRanking);
			}
			return Observable.Select<TournamentRanking, TournamentRanking>(tournamentRankByTournamentIdDetailed, SwordfishTournamentRankingProvider.<>f__mg$cache0);
		}

		public IObservable<TournamentRanking> GetStepRanking(long stepId, int numberOfPositions)
		{
			IObservable<TournamentRanking> stepRankByStepIdDetailed = TournamentCustomWS.GetStepRankByStepIdDetailed(new TournamentStepRankingParameters
			{
				StepId = stepId,
				RankAmount = (long)numberOfPositions
			});
			if (SwordfishTournamentRankingProvider.<>f__mg$cache1 == null)
			{
				SwordfishTournamentRankingProvider.<>f__mg$cache1 = new Func<TournamentRanking, TournamentRanking>(SwordfishTournamentRankingProvider.ConvertToTournamentRanking);
			}
			return Observable.Select<TournamentRanking, TournamentRanking>(stepRankByStepIdDetailed, SwordfishTournamentRankingProvider.<>f__mg$cache1);
		}

		public IObservable<TournamentRankingPosition> GetTeamGeneralRankingPosition(long tournamentId, Guid teamId)
		{
			IObservable<TournamentRank> myTeamTournamentRankByTournamentIdDetailed = TournamentCustomWS.GetMyTeamTournamentRankByTournamentIdDetailed(tournamentId);
			if (SwordfishTournamentRankingProvider.<>f__mg$cache2 == null)
			{
				SwordfishTournamentRankingProvider.<>f__mg$cache2 = new Func<TournamentRank, TournamentRankingPosition>(SwordfishTournamentRankingProvider.ConvertToTournamentRankingPositionIfValid);
			}
			return Observable.Select<TournamentRank, TournamentRankingPosition>(myTeamTournamentRankByTournamentIdDetailed, SwordfishTournamentRankingProvider.<>f__mg$cache2);
		}

		public IObservable<TournamentRankingPosition> GetTeamStepRankingPosition(long stepId, Guid teamId)
		{
			IObservable<TournamentRank> myTeamStepRankByStepIdDetailed = TournamentCustomWS.GetMyTeamStepRankByStepIdDetailed(stepId);
			if (SwordfishTournamentRankingProvider.<>f__mg$cache3 == null)
			{
				SwordfishTournamentRankingProvider.<>f__mg$cache3 = new Func<TournamentRank, TournamentRankingPosition>(SwordfishTournamentRankingProvider.ConvertToTournamentRankingPositionIfValid);
			}
			return Observable.Select<TournamentRank, TournamentRankingPosition>(myTeamStepRankByStepIdDetailed, SwordfishTournamentRankingProvider.<>f__mg$cache3);
		}

		private static TournamentRanking ConvertToTournamentRanking(TournamentRanking swordfishRanking)
		{
			TournamentRanking tournamentRanking = new TournamentRanking
			{
				Positions = new TournamentRankingPosition[swordfishRanking.Ranks.Length]
			};
			for (int i = 0; i < swordfishRanking.Ranks.Length; i++)
			{
				TournamentRank rank = swordfishRanking.Ranks[i];
				tournamentRanking.Positions[i] = SwordfishTournamentRankingProvider.ConvertToTournamentRankingPositionIfValid(rank);
			}
			return tournamentRanking;
		}

		private static TournamentRankingPosition ConvertToTournamentRankingPositionIfValid(TournamentRank rank)
		{
			if (rank.TeamId == Guid.Empty)
			{
				throw new NoRankingForTeamException();
			}
			long num = rank.TiebreakInfo.GetAverageMatchTime();
			if (num == 9223372036854775807L)
			{
				num = 0L;
			}
			return new TournamentRankingPosition
			{
				TeamId = rank.TeamId,
				Points = rank.TotalPoints,
				Position = rank.Position,
				TeamIconAssetName = rank.TeamIconAssetName,
				TeamMembersName = rank.TeamMembersName,
				ClassificatoryPoints = rank.ClassificatoryPoints,
				TeamTag = rank.TeamTag,
				CurrentUgmUserUniversalId = rank.CurrentUgmUserUniversalId,
				TeamName = rank.TeamName,
				UserGeneratedContentCurrentOwnerPlayerId = rank.UserGeneratedContentCurrentOwnerPlayerId,
				TieBreakerAverageGoalDiff = rank.TiebreakInfo.GetGoalRatio(),
				TieBreakerAverageMatchDuration = TimeSpan.FromMilliseconds((double)num)
			};
		}

		[CompilerGenerated]
		private static Func<TournamentRanking, TournamentRanking> <>f__mg$cache0;

		[CompilerGenerated]
		private static Func<TournamentRanking, TournamentRanking> <>f__mg$cache1;

		[CompilerGenerated]
		private static Func<TournamentRank, TournamentRankingPosition> <>f__mg$cache2;

		[CompilerGenerated]
		private static Func<TournamentRank, TournamentRankingPosition> <>f__mg$cache3;
	}
}
