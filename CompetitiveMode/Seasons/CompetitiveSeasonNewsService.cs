using System;
using HeavyMetalMachines.CompetitiveMode.Infra.News;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Seasons
{
	public class CompetitiveSeasonNewsService : ICompetitiveSeasonNewsService
	{
		public CompetitiveSeasonNewsService(ITryConsumeAndGetSeasonNews tryConsumeAndGetSeasonNews)
		{
			this._tryConsumeAndGetSeasonNews = tryConsumeAndGetSeasonNews;
		}

		public IObservable<bool> TryConsume(long seasonId)
		{
			bool flag = this._tryConsumeAndGetSeasonNews.TryConsumeAndGetCompetitive(seasonId);
			return Observable.Return(flag);
		}

		private readonly ITryConsumeAndGetSeasonNews _tryConsumeAndGetSeasonNews;
	}
}
