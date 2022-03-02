using System;
using HeavyMetalMachines.CompetitiveMode.View.Matches;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Seasons
{
	public class UpdateSeasons : IUpdateSeasons
	{
		public UpdateSeasons(ISeasonsStorage seasonsStorage, ICompetitiveSeasonsProvider competitiveSeasonsProvider, ILogger<UpdateSeasons> logger)
		{
			this._seasonsStorage = seasonsStorage;
			this._competitiveSeasonsProvider = competitiveSeasonsProvider;
			this._logger = logger;
		}

		public IObservable<Unit> Update()
		{
			return Observable.AsUnitObservable<UpdateSeasons.Seasons>(Observable.Do<UpdateSeasons.Seasons>(Observable.Do<UpdateSeasons.Seasons>(Observable.ContinueWith<UpdateSeasons.Seasons, UpdateSeasons.Seasons>(Observable.ContinueWith<UpdateSeasons.Seasons, UpdateSeasons.Seasons>(Observable.ContinueWith<UpdateSeasons.Seasons, UpdateSeasons.Seasons>(Observable.Return<UpdateSeasons.Seasons>(new UpdateSeasons.Seasons()), new Func<UpdateSeasons.Seasons, IObservable<UpdateSeasons.Seasons>>(this.FetchCurrentSeason)), new Func<UpdateSeasons.Seasons, IObservable<UpdateSeasons.Seasons>>(this.FetchNextSeasonIfThereIsNoCurrentSeason)), new Func<UpdateSeasons.Seasons, IObservable<UpdateSeasons.Seasons>>(this.GetFakeSeasonIfProviderHasNoSeason)), new Action<UpdateSeasons.Seasons>(this.StoreSeasons)), new Action<UpdateSeasons.Seasons>(this.LogSeasons)));
		}

		private void LogSeasons(UpdateSeasons.Seasons seasons)
		{
			this._logger.InfoFormat("CurrentSeason=[{0}], NextSeason=[{1}]", new object[]
			{
				this.TryGetSeasonDates(seasons.Current),
				this.TryGetSeasonDates(seasons.Next)
			});
		}

		private string TryGetSeasonDates(CompetitiveSeason season)
		{
			if (season == null)
			{
				return "null";
			}
			return string.Format("Id={0}, Start={1}, End={2}", season.Id, season.StartDateTime, season.EndDateTime);
		}

		private IObservable<UpdateSeasons.Seasons> FetchCurrentSeason(UpdateSeasons.Seasons seasons)
		{
			return Observable.Select<CompetitiveSeason, UpdateSeasons.Seasons>(Observable.Do<CompetitiveSeason>(this._competitiveSeasonsProvider.GetCurrentSeason(), delegate(CompetitiveSeason currentSeason)
			{
				seasons.Current = currentSeason;
			}), (CompetitiveSeason _) => seasons);
		}

		private IObservable<UpdateSeasons.Seasons> FetchNextSeasonIfThereIsNoCurrentSeason(UpdateSeasons.Seasons seasons)
		{
			if (seasons.Current != null)
			{
				return Observable.Return<UpdateSeasons.Seasons>(seasons);
			}
			return Observable.Select<CompetitiveSeason, UpdateSeasons.Seasons>(Observable.Do<CompetitiveSeason>(this._competitiveSeasonsProvider.GetNextSeason(), delegate(CompetitiveSeason nextSeason)
			{
				seasons.Next = nextSeason;
			}), (CompetitiveSeason _) => seasons);
		}

		private IObservable<UpdateSeasons.Seasons> GetFakeSeasonIfProviderHasNoSeason(UpdateSeasons.Seasons seasons)
		{
			if (seasons.Current != null || seasons.Next != null)
			{
				return Observable.Return<UpdateSeasons.Seasons>(seasons);
			}
			this._logger.Warn("There is no current or next competitive season from provider. Fallbacking to a fake season. Competitive matchmaking may not work in this state.");
			FakeCompetitiveSeasonsProvider fakeCompetitiveSeasonsProvider = new FakeCompetitiveSeasonsProvider();
			return Observable.ContinueWith<CompetitiveSeason, UpdateSeasons.Seasons>(Observable.Merge<CompetitiveSeason>(new IObservable<CompetitiveSeason>[]
			{
				Observable.Do<CompetitiveSeason>(fakeCompetitiveSeasonsProvider.GetCurrentSeason(), delegate(CompetitiveSeason currentSeason)
				{
					seasons.Current = currentSeason;
				}),
				Observable.Do<CompetitiveSeason>(fakeCompetitiveSeasonsProvider.GetNextSeason(), delegate(CompetitiveSeason nextSeason)
				{
					seasons.Next = nextSeason;
				})
			}), Observable.Return<UpdateSeasons.Seasons>(seasons));
		}

		private void StoreSeasons(UpdateSeasons.Seasons seasons)
		{
			this._seasonsStorage.CurrentSeason = seasons.Current;
			this._seasonsStorage.NextSeason = seasons.Next;
		}

		private readonly ISeasonsStorage _seasonsStorage;

		private readonly ICompetitiveSeasonsProvider _competitiveSeasonsProvider;

		private readonly ILogger<UpdateSeasons> _logger;

		private class Seasons
		{
			public CompetitiveSeason Current { get; set; }

			public CompetitiveSeason Next { get; set; }
		}
	}
}
