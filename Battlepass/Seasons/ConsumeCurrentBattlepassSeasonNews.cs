using System;
using UniRx;

namespace HeavyMetalMachines.Battlepass.Seasons
{
	public class ConsumeCurrentBattlepassSeasonNews : IConsumeCurrentBattlepassSeasonNews
	{
		public ConsumeCurrentBattlepassSeasonNews(IBattlepassDetailComponent battlepassDetailComponent, IBattlepassSeasonNewsService battlepassSeasonNewsService)
		{
			this._battlepassDetailComponent = battlepassDetailComponent;
			this._battlepassSeasonNewsService = battlepassSeasonNewsService;
		}

		public IObservable<bool> TryConsume()
		{
			long seasonId = (long)this._battlepassDetailComponent.BattlepassConfig.Season;
			return this._battlepassSeasonNewsService.TryConsume(seasonId);
		}

		private readonly IBattlepassDetailComponent _battlepassDetailComponent;

		private readonly IBattlepassSeasonNewsService _battlepassSeasonNewsService;
	}
}
