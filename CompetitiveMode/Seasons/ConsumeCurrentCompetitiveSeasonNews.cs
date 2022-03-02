using System;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Seasons
{
	public class ConsumeCurrentCompetitiveSeasonNews : IConsumeCurrentCompetitiveSeasonNews
	{
		public ConsumeCurrentCompetitiveSeasonNews(ICompetitiveSeasonNewsService competitiveSeasonNewsService, ISeasonsStorage seasonsStorage)
		{
			this._competitiveSeasonNewsService = competitiveSeasonNewsService;
			this._seasonsStorage = seasonsStorage;
		}

		public IObservable<bool> TryConsume()
		{
			if (this._seasonsStorage.CurrentSeason == null)
			{
				return Observable.Return(false);
			}
			long id = this._seasonsStorage.CurrentSeason.Id;
			return this._competitiveSeasonNewsService.TryConsume(id);
		}

		private readonly ICompetitiveSeasonNewsService _competitiveSeasonNewsService;

		private readonly ISeasonsStorage _seasonsStorage;
	}
}
