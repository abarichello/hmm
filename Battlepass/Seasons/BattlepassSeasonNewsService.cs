using System;
using HeavyMetalMachines.CompetitiveMode.Infra.News;
using UniRx;

namespace HeavyMetalMachines.Battlepass.Seasons
{
	public class BattlepassSeasonNewsService : IBattlepassSeasonNewsService
	{
		public BattlepassSeasonNewsService(ITryConsumeAndGetSeasonNews tryConsumeAndGetSeasonNews)
		{
			this._tryConsumeAndGetSeasonNews = tryConsumeAndGetSeasonNews;
		}

		public IObservable<bool> TryConsume(long seasonId)
		{
			bool flag = this._tryConsumeAndGetSeasonNews.TryConsumeAndGetBattlepass(seasonId);
			return Observable.Return(flag);
		}

		private readonly ITryConsumeAndGetSeasonNews _tryConsumeAndGetSeasonNews;
	}
}
