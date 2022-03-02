using System;
using System.Collections.Generic;
using System.Linq;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.CompetitiveMode.Seasons;
using HeavyMetalMachines.CompetitiveMode.Seasons.Exceptions;
using HeavyMetalMachines.Crossplay;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Publishing;
using HeavyMetalMachines.Social.Friends.Business;
using HeavyMetalMachines.Social.Friends.Models;
using Hoplon.Assertions;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Ranking
{
	public class GetCompetitiveFriendsRanking : IGetCompetitiveFriendsRanking
	{
		public GetCompetitiveFriendsRanking(ISeasonsStorage seasonsStorage, IGetFriends getFriends, ICompetitiveRankingProvider competitiveRankingProvider, ILocalPlayerStorage playerStorage, IIsCrossplayEnabled isCrossplayEnabled, ILogger<GetCompetitiveFriendsRanking> logger)
		{
			Assert.ConstructorParametersAreNotNull(new object[]
			{
				seasonsStorage,
				getFriends,
				competitiveRankingProvider,
				playerStorage,
				isCrossplayEnabled,
				logger
			});
			this._seasonsStorage = seasonsStorage;
			this._getFriends = getFriends;
			this._competitiveRankingProvider = competitiveRankingProvider;
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
			Friend[] array = this._getFriends.Get();
			if (array.Length == 0)
			{
				return Observable.Return<PlayerCompetitiveRankingPosition[]>(new PlayerCompetitiveRankingPosition[0]);
			}
			long seasonId = this._seasonsStorage.CurrentSeason.Id;
			long playerId = this._playerStorage.Player.PlayerId;
			IEnumerable<long> first = from friend in array
			select friend.PlayerId;
			IEnumerable<long> source = first.Concat(new long[]
			{
				playerId
			});
			return Observable.Select<PlayerCompetitiveRankingPosition[], PlayerCompetitiveRankingPosition[]>(Observable.Do<PlayerCompetitiveRankingPosition[]>(Observable.Do<PlayerCompetitiveRankingPosition[]>(Observable.Catch<PlayerCompetitiveRankingPosition[], Exception>(this._competitiveRankingProvider.GetPlayersRanking(seasonId, numberOfPlayers, source.ToArray<long>()), (Exception exception) => this.LogErrorAndFallbackToEmptyRanking(exception, seasonId)), new Action<PlayerCompetitiveRankingPosition[]>(this.MarkLocalPlayerIfExisting)), new Action<PlayerCompetitiveRankingPosition[]>(this.ReplacePositionsWithSequentialPositions)), new Func<PlayerCompetitiveRankingPosition[], PlayerCompetitiveRankingPosition[]>(this.FilterRankingForCrossplay));
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

		private void ReplacePositionsWithSequentialPositions(PlayerCompetitiveRankingPosition[] rankingPositions)
		{
			for (int i = 0; i < rankingPositions.Length; i++)
			{
				rankingPositions[i].Position = (long)(i + 1);
			}
		}

		private IObservable<PlayerCompetitiveRankingPosition[]> LogErrorAndFallbackToEmptyRanking(Exception exception, long seasonId)
		{
			this._logger.Error(exception);
			this._logger.Warn(string.Format("An error occurred when trying to fetch players competitive ranking for season {0}. Fallbacking to empty list.", seasonId));
			return Observable.Return<PlayerCompetitiveRankingPosition[]>(new PlayerCompetitiveRankingPosition[0]);
		}

		private readonly ISeasonsStorage _seasonsStorage;

		private readonly IGetFriends _getFriends;

		private readonly ICompetitiveRankingProvider _competitiveRankingProvider;

		private readonly ILocalPlayerStorage _playerStorage;

		private readonly IIsCrossplayEnabled _isCrossplayEnabled;

		private readonly ILogger<GetCompetitiveFriendsRanking> _logger;
	}
}
