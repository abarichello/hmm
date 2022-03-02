using System;
using HeavyMetalMachines.Tournaments.Infra;
using HeavyMetalMachines.Tournaments.Ranking;
using UniRx;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishTournamentRankingProvider : ITournamentRankingProvider
	{
		public IObservable<TournamentRanking> GetGeneralRanking(long tournamentId, int numberOfPositions)
		{
			return Observable.Empty<TournamentRanking>();
		}

		public IObservable<TournamentRanking> GetStepRanking(long tournamentId, int numberOfPositions)
		{
			return Observable.Empty<TournamentRanking>();
		}

		public IObservable<TournamentRankingPosition> GetTeamGeneralRankingPosition(long tournamentId, Guid teamId)
		{
			return Observable.Empty<TournamentRankingPosition>();
		}

		public IObservable<TournamentRankingPosition> GetTeamStepRankingPosition(long stepId, Guid teamId)
		{
			return Observable.Empty<TournamentRankingPosition>();
		}
	}
}
