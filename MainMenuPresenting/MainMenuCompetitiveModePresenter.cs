using System;
using HeavyMetalMachines.CompetitiveMode.Matchmaking;
using HeavyMetalMachines.CompetitiveMode.View.Prizes;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting;
using Hoplon.Localization.TranslationTable;
using UniRx;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public class MainMenuCompetitiveModePresenter : IMainMenuCompetitiveModePresenter, IDisposable
	{
		public MainMenuCompetitiveModePresenter(ICompetitiveSeasonRewardsCollectionPresenter competitiveSeasonRewardsCollectionPresenter, IContinuouslyCheckAndCancelCompetitiveMatchSearch continuouslyCheckAndCancelCompetitiveMatchSearch, IDialogPresenter dialogPresenter, ILocalizeKey translation)
		{
			this._competitiveSeasonRewardsCollectionPresenter = competitiveSeasonRewardsCollectionPresenter;
			this._continuouslyCheckAndCancelCompetitiveMatchSearch = continuouslyCheckAndCancelCompetitiveMatchSearch;
			this._dialogPresenter = dialogPresenter;
			this._translation = translation;
		}

		public IObservable<Unit> Initialize()
		{
			this._checkAndCancelMatchSearchDisposable = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._continuouslyCheckAndCancelCompetitiveMatchSearch.CheckAndCancel(), delegate(Unit _)
			{
				this.ShowSearchCanceledDialog();
			}));
			return this._competitiveSeasonRewardsCollectionPresenter.Initialize();
		}

		private void ShowSearchCanceledDialog()
		{
			DialogConfiguration dialogConfiguration = new DialogConfiguration
			{
				Message = this._translation.Get("RANKED_END_SEASON_FEEDBACK", TranslationContext.Ranked)
			};
			ObservableExtensions.Subscribe<Unit>(this._dialogPresenter.Show(dialogConfiguration));
		}

		public IObservable<Unit> Show()
		{
			if (!this.ShouldShow())
			{
				return Observable.ReturnUnit();
			}
			return this._competitiveSeasonRewardsCollectionPresenter.Show();
		}

		public void Dispose()
		{
			if (this._checkAndCancelMatchSearchDisposable != null)
			{
				this._checkAndCancelMatchSearchDisposable.Dispose();
			}
		}

		private bool ShouldShow()
		{
			return this._competitiveSeasonRewardsCollectionPresenter.ShouldShow();
		}

		private readonly ICompetitiveSeasonRewardsCollectionPresenter _competitiveSeasonRewardsCollectionPresenter;

		private readonly IContinuouslyCheckAndCancelCompetitiveMatchSearch _continuouslyCheckAndCancelCompetitiveMatchSearch;

		private readonly IDialogPresenter _dialogPresenter;

		private readonly ILocalizeKey _translation;

		private IDisposable _checkAndCancelMatchSearchDisposable;
	}
}
