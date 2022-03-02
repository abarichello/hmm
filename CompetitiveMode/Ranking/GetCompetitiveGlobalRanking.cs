using System;
using System.Linq;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.CompetitiveMode.Seasons;
using HeavyMetalMachines.CompetitiveMode.Seasons.Exceptions;
using HeavyMetalMachines.Crossplay;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Publishing;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Ranking
{
	public class GetCompetitiveGlobalRanking : IGetCompetitiveGlobalRanking
	{
		public GetCompetitiveGlobalRanking(ICompetitiveRankingProvider competitiveRankingProvider, ISeasonsStorage seasonsStorage, ILocalPlayerStorage playerStorage, IIsCrossplayEnabled isCrossplayEnabled, ILogger<GetCompetitiveGlobalRanking> logger)
		{
			this._competitiveRankingProvider = competitiveRankingProvider;
			this._seasonsStorage = seasonsStorage;
			this._playerStorage = playerStorage;
			this._isCrossplayEnabled = isCrossplayEnabled;
			this._logger = logger;
		}

		public IObservable<PlayerCompetitiveRankingPosition[]> Get(int numberOfPlayers)
		{
			if (this._seasonsStorage.CurrentSeason == null)
			{
				throw new NoCurrentCompetitiveSeasonException();
			}
			long seasonId = this._seasonsStorage.CurrentSeason.Id;
			return Observable.Select<PlayerCompetitiveRankingPosition[], PlayerCompetitiveRankingPosition[]>(Observable.Do<PlayerCompetitiveRankingPosition[]>(Observable.Catch<PlayerCompetitiveRankingPosition[], Exception>(this._competitiveRankingProvider.GetGlobalRanking(seasonId, numberOfPlayers), (Exception exception) => this.LogErrorAndFallbackToEmptyRanking(exception, seasonId)), new Action<PlayerCompetitiveRankingPosition[]>(this.MarkLocalPlayerIfExisting)), new Func<PlayerCompetitiveRankingPosition[], PlayerCompetitiveRankingPosition[]>(this.FilterRankingForCrossplay));
		}

		private PlayerCompetitiveRankingPosition[] FilterRankingForCrossplay(PlayerCompetitiveRankingPosition[] rankingList)
		{
			if (this._isCrossplayEnabled.Get())
			{
				return rankingList;
			}
			return (from rankingMember in rankingList
			where rankingMember.Publisher == Publishers.Psn.SwordfishUniqueName
			select rankingMember).ToArray<PlayerCompetitiveRankingPosition>();
		}

		private void MarkLocalPlayerIfExisting(PlayerCompetitiveRankingPosition[] rankingPositionList)
		{
			long playerId = this._playerStorage.Player.PlayerId;
			foreach (PlayerCompetitiveRankingPosition playerCompetitiveRankingPosition in rankingPositionList)
			{
				if (playerCompetitiveRankingPosition.PlayerId == playerId)
				{
					playerCompetitiveRankingPosition.IsLocalPlayer = true;
					return;
				}
			}
		}

		private IObservable<PlayerCompetitiveRankingPosition[]> LogErrorAndFallbackToEmptyRanking(Exception exception, long seasonId)
		{
			this._logger.Error(exception);
			this._logger.Warn(string.Format("An error occurred when trying to fetch global competitive ranking for season {0}. Fallbacking to empty list.", seasonId));
			return Observable.Return<PlayerCompetitiveRankingPosition[]>(new PlayerCompetitiveRankingPosition[0]);
		}

		private readonly ICompetitiveRankingProvider _competitiveRankingProvider;

		private readonly ISeasonsStorage _seasonsStorage;

		private readonly ILocalPlayerStorage _playerStorage;

		private readonly IIsCrossplayEnabled _isCrossplayEnabled;

		private readonly ILogger<GetCompetitiveGlobalRanking> _logger;
	}
}
