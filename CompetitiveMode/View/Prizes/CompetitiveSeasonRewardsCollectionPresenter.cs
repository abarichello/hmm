using System;
using System.Linq;
using HeavyMetalMachines.CompetitiveMode.Prizes;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting;
using Hoplon.Localization.TranslationTable;
using Hoplon.Reactive;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.View.Prizes
{
	public class CompetitiveSeasonRewardsCollectionPresenter : ICompetitiveSeasonRewardsCollectionPresenter, IPresenter
	{
		public CompetitiveSeasonRewardsCollectionPresenter(ITryCollectMyPlayerPendingCompetitivePrizes tryCollectMyPlayerPendingPrizes, ICompetitiveDivisionsBadgeNameBuilder divisionsBadgeNameBuilder, ILocalizeKey translation, IViewProvider viewProvider, IViewLoader viewLoader)
		{
			this._tryCollectMyPlayerPendingPrizes = tryCollectMyPlayerPendingPrizes;
			this._divisionsBadgeNameBuilder = divisionsBadgeNameBuilder;
			this._translation = translation;
			this._viewProvider = viewProvider;
			this._viewLoader = viewLoader;
		}

		public IObservable<Unit> Initialize()
		{
			return ObservableExtensions.IfElse<CompetitivePrizesCollection, Unit>(this._tryCollectMyPlayerPendingPrizes.TryCollect(), (CompetitivePrizesCollection prizesCollection) => prizesCollection.HasCollected, new Func<CompetitivePrizesCollection, IObservable<Unit>>(this.InitializeScene), (CompetitivePrizesCollection _) => Observable.ReturnUnit());
		}

		private IObservable<Unit> InitializeScene(CompetitivePrizesCollection prizesCollection)
		{
			return Observable.AsUnitObservable<Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._hasCollectedAnyPrizes = true;
			}), (Unit _) => this._viewLoader.LoadView("UI_ADD_RankingEnd")), delegate(Unit _)
			{
				this.InitializeView(prizesCollection);
			}));
		}

		private void InitializeView(CompetitivePrizesCollection prizesCollection)
		{
			this._view = this._viewProvider.Provide<ICompetitiveSeasonRewardsCollectionView>(null);
			this._view.MainCanvas.Enable();
			this.InitializeSeason(prizesCollection);
			this.InitializeDivision(prizesCollection);
			this.InitializePrizes(prizesCollection);
		}

		private void InitializeSeason(CompetitivePrizesCollection prizesCollection)
		{
			this._view.SeasonNameLabel.Text = string.Format(this._view.SeasonNameLabel.Text, prizesCollection.Season.Id);
		}

		private void InitializeDivision(CompetitivePrizesCollection prizesCollection)
		{
			this._view.DivisionNameLabel.Text = this._divisionsBadgeNameBuilder.GetDivisionWithSubdivisionNameTranslated(prizesCollection.PrizeRank);
			string text = prizesCollection.PrizeRank.Score.ToString();
			this._view.ScoreLabel.Text = this._translation.GetFormatted("RANKING_SCORE_ABBREVIATION", TranslationContext.Ranked, new object[]
			{
				text
			});
			if (prizesCollection.PrizeRank.TopPlacementPosition != null)
			{
				IActivatable activatable = this._view.DivisionGroups.Last<IActivatable>();
				ActivatableExtensions.Activate(activatable);
			}
			else
			{
				string subdivisionBadgeFileName = this._divisionsBadgeNameBuilder.GetSubdivisionBadgeFileName(prizesCollection.PrizeRank, 700);
				int division = prizesCollection.PrizeRank.Division;
				IDynamicImage dynamicImage = this._view.DivisionImages[division];
				IActivatable activatable2 = this._view.DivisionGroups[division];
				dynamicImage.SetImageName(subdivisionBadgeFileName);
				ActivatableExtensions.Activate(activatable2);
			}
		}

		private void InitializePrizes(CompetitivePrizesCollection prizesCollection)
		{
			foreach (CollectedPrize collectedPrize in prizesCollection.CollectedPrizes)
			{
				ICompetitiveSeasonRewardCollectionItemView competitiveSeasonRewardCollectionItemView = this._view.CreateAndAddItem();
				competitiveSeasonRewardCollectionItemView.ThumbnailImage.SetImageName(collectedPrize.Prize.ThumbnailImageName);
				competitiveSeasonRewardCollectionItemView.NameLabel.Text = this._translation.Get(collectedPrize.Prize.TitleDraft, TranslationContext.Items);
			}
		}

		public IObservable<Unit> Show()
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(this._view.ShowAnimation.Play(), delegate(Unit _)
			{
				this._view.UiNavigationGroupHolder.AddGroup();
			}), (Unit _) => Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.First<Unit>(this._view.ConfirmButton.OnClick()), (Unit __) => this.Hide()), (Unit __) => this.Dispose()));
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Do<Unit>(Observable.Do<Unit>(this._view.HideAnimation.Play(), delegate(Unit _)
			{
				this._view.UiNavigationGroupHolder.RemoveGroup();
			}), delegate(Unit _)
			{
				this._hideSubject.OnNext(Unit.Default);
			});
		}

		public IObservable<Unit> Dispose()
		{
			return this._viewLoader.UnloadView("UI_ADD_RankingEnd");
		}

		public IObservable<Unit> ObserveHide()
		{
			return this._hideSubject;
		}

		public bool ShouldShow()
		{
			return this._hasCollectedAnyPrizes;
		}

		private const string SceneName = "UI_ADD_RankingEnd";

		private readonly ITryCollectMyPlayerPendingCompetitivePrizes _tryCollectMyPlayerPendingPrizes;

		private readonly ICompetitiveDivisionsBadgeNameBuilder _divisionsBadgeNameBuilder;

		private readonly ILocalizeKey _translation;

		private readonly IViewProvider _viewProvider;

		private readonly IViewLoader _viewLoader;

		private readonly Subject<Unit> _hideSubject = new Subject<Unit>();

		private ICompetitiveSeasonRewardsCollectionView _view;

		private bool _hasCollectedAnyPrizes;
	}
}
