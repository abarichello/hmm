using System;
using System.Linq;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.CompetitiveMode.DataTransferObjects.Ranking;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.CompetitiveMode.Ranking;
using HeavyMetalMachines.Swordfish;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Infra
{
	public class SwordfishCompetitiveRankingProvider : ICompetitiveRankingProvider
	{
		public SwordfishCompetitiveRankingProvider(ICustomWS customWs)
		{
			this._customWs = customWs;
		}

		public IObservable<PlayerCompetitiveRankingPosition[]> GetGlobalRanking(long seasonId, int numberOfPlayers)
		{
			GlobalRankingParameters globalRankingParameters = new GlobalRankingParameters
			{
				SeasonId = seasonId,
				NumberOfPlayers = numberOfPlayers
			};
			return Observable.Select<SerializablePlayerCompetitiveRankingPositionCollection, PlayerCompetitiveRankingPosition[]>(this._customWs.ExecuteAsObservable("GetCompetitiveGlobalRanking", (string)globalRankingParameters), new Func<SerializablePlayerCompetitiveRankingPositionCollection, PlayerCompetitiveRankingPosition[]>(this.ToPlayerCompetitiveRankings));
		}

		public IObservable<PlayerCompetitiveRankingPosition[]> GetPlayersRanking(long seasonId, int numberOfPlayers, long[] playersIds)
		{
			PlayersRankingParameters playersRankingParameters = new PlayersRankingParameters
			{
				SeasonId = seasonId,
				NumberOfPlayers = numberOfPlayers,
				PlayersIds = playersIds
			};
			return Observable.Select<SerializablePlayerCompetitiveRankingPositionCollection, PlayerCompetitiveRankingPosition[]>(this._customWs.ExecuteAsObservable("GetCompetitivePlayersRanking", (string)playersRankingParameters), new Func<SerializablePlayerCompetitiveRankingPositionCollection, PlayerCompetitiveRankingPosition[]>(this.ToPlayerCompetitiveRankings));
		}

		private PlayerCompetitiveRankingPosition[] ToPlayerCompetitiveRankings(SerializablePlayerCompetitiveRankingPositionCollection collection)
		{
			return collection.RankingsPositions.Select(new Func<SerializablePlayerCompetitiveRankingPosition, PlayerCompetitiveRankingPosition>(this.ToPlayerCompetitiveRanking)).ToArray<PlayerCompetitiveRankingPosition>();
		}

		private PlayerCompetitiveRankingPosition ToPlayerCompetitiveRanking(SerializablePlayerCompetitiveRankingPosition ranking)
		{
			PlayerCompetitiveRankingPosition playerCompetitiveRankingPosition = new PlayerCompetitiveRankingPosition();
			playerCompetitiveRankingPosition.PlayerId = ranking.PlayerId;
			playerCompetitiveRankingPosition.Name = ranking.PlayerName;
			playerCompetitiveRankingPosition.PlayerTag = ranking.PlayerTag;
			playerCompetitiveRankingPosition.Position = ranking.Position;
			PlayerCompetitiveRankingPosition playerCompetitiveRankingPosition2 = playerCompetitiveRankingPosition;
			CompetitiveRank rank = default(CompetitiveRank);
			rank.Division = ranking.Rank.Division;
			rank.Subdivision = ranking.Rank.Subdivision;
			rank.Score = ranking.Rank.Score;
			rank.TopPlacementPosition = ranking.Rank.TopPlacementPosition;
			playerCompetitiveRankingPosition2.Rank = rank;
			playerCompetitiveRankingPosition.UniversalId = ranking.UniversalId;
			playerCompetitiveRankingPosition.Publisher = ranking.Publisher;
			playerCompetitiveRankingPosition.IsCrossplayEnable = ranking.IsCrossplayEnable;
			return playerCompetitiveRankingPosition;
		}

		private readonly ICustomWS _customWs;
	}
}
