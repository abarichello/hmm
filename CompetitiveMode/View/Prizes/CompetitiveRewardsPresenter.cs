using System;
using System.Collections.Generic;
using System.Linq;
using HeavyMetalMachines.CompetitiveMode.Divisions;
using HeavyMetalMachines.CompetitiveMode.Prizes;
using HeavyMetalMachines.CompetitiveMode.Seasons;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Extensions;
using HeavyMetalMachines.UnityUI;
using Hoplon.Localization.TranslationTable;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.View.Prizes
{
	public class CompetitiveRewardsPresenter : ICompetitiveRewardsPresenter, IPresenter
	{
		public CompetitiveRewardsPresenter(IViewLoader viewLoader, IViewProvider viewProvider, ILocalizeKey translation, IGetCurrentOrNextCompetitiveSeason getCurrentOrNextCompetitiveSeason, IGetCompetitiveDivisions getCompetitiveDivisions, IGetCompetitiveDivisionsPrizes getCompetitiveDivisionsPrizes, ICompetitiveDivisionsBadgeNameBuilder divisionsBadgeNameBuilder, IMainMenuPresenterTree mainMenuPresenterTree)
		{
			this._viewLoader = viewLoader;
			this._viewProvider = viewProvider;
			this._translation = translation;
			this._getCurrentOrNextCompetitiveSeason = getCurrentOrNextCompetitiveSeason;
			this._getCompetitiveDivisions = getCompetitiveDivisions;
			this._getCompetitiveDivisionsPrizes = getCompetitiveDivisionsPrizes;
			this._divisionsBadgeNameBuilder = divisionsBadgeNameBuilder;
			this._mainMenuPresenterTree = mainMenuPresenterTree;
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.Do<Unit>(this._viewLoader.LoadView("UI_ADD_RankingReward"), delegate(Unit _)
			{
				this.InitializeView();
			});
		}

		private void InitializeView()
		{
			this._view = this._viewProvider.Provide<ICompetitiveRewardsView>(null);
			this._view.MainCanvas.Enable();
			this.InitializeDivisions();
			this.InitializeTitle();
			this.InitializeBackButton();
			this.InitializeDivisionChangeButtons();
		}

		private void InitializeDivisions()
		{
			this._divisions = this._getCompetitiveDivisions.Get();
			this._currentDivisionIndex = 0;
			this.UpdateDivisionView();
			this.UpdateTemporaryFields();
			this._view.PreviousDivisionButton.IsInteractable = this.HasPreviousDivision();
			this._view.NextDivisionButton.IsInteractable = this.HasNextDivision();
		}

		private void InitializeTitle()
		{
			this._view.Title.Title = this._translation.Get("RANKING_REWARD_TITLE_WINDOW", TranslationContext.Ranked);
			this._view.Title.Description = this._translation.Get("RANKING_REWARD_DESCRIPTION_WINDOW", TranslationContext.Ranked);
			ActivatableExtensions.Activate(this._view.Title.DescriptionActivatable);
			ActivatableExtensions.Deactivate(this._view.Title.SubtitleActivatable);
			ActivatableExtensions.Deactivate(this._view.Title.InfoButton);
		}

		private void InitializeBackButton()
		{
			ObservableExtensions.Subscribe<Unit>(ButtonExtensions.OnClickNavigateBackwards(this._view.BackButton, this._mainMenuPresenterTree.PresenterTree));
		}

		private void InitializeDivisionChangeButtons()
		{
			ObservableExtensions.Subscribe<Unit>(Observable.Switch<Unit>(Observable.Select<Unit, IObservable<Unit>>(Observable.Where<Unit>(this._view.PreviousDivisionButton.OnClick(), (Unit _) => this.HasPreviousDivision()), (Unit _) => this.ChangeToPreviousDivision())));
			ObservableExtensions.Subscribe<Unit>(Observable.Switch<Unit>(Observable.Select<Unit, IObservable<Unit>>(Observable.Where<Unit>(this._view.NextDivisionButton.OnClick(), (Unit _) => this.HasNextDivision()), (Unit _) => this.ChangeToNextDivision())));
		}

		private bool HasPreviousDivision()
		{
			return this._currentDivisionIndex > 0;
		}

		private bool HasNextDivision()
		{
			return this._currentDivisionIndex < this._divisions.Length;
		}

		private bool IsShowingTopPlacement()
		{
			return this._currentDivisionIndex == this._divisions.Length;
		}

		private IObservable<Unit> ChangeToPreviousDivision()
		{
			this._currentDivisionIndex--;
			this._view.PreviousDivisionButton.IsInteractable = this.HasPreviousDivision();
			this._view.NextDivisionButton.IsInteractable = true;
			return this.UpdateViewToCurrentDivision();
		}

		private IObservable<Unit> ChangeToNextDivision()
		{
			this._currentDivisionIndex++;
			this._view.PreviousDivisionButton.IsInteractable = true;
			this._view.NextDivisionButton.IsInteractable = this.HasNextDivision();
			return this.UpdateViewToCurrentDivision();
		}

		private IObservable<Unit> UpdateViewToCurrentDivision()
		{
			IObservable<Unit> observable = Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(this._view.DivisionHideAnimation.Play(), delegate(Unit _)
			{
				this.UpdateDivisionView();
			}), (Unit _) => this._view.DivisionShowAnimation.Play());
			IObservable<Unit> observable2 = Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(this._view.TemporaryPreviewHideAnimation.Play(), delegate(Unit _)
			{
				this.UpdateTemporaryFields();
			}), (Unit _) => this._view.TemporaryPreviewShowAnimation.Play());
			return Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				observable,
				observable2
			});
		}

		private void UpdateDivisionView()
		{
			this.UpdateDivisionDescription();
		}

		private void UpdateTemporaryFields()
		{
			if (this.IsShowingTopPlacement())
			{
				int num = this._view.TemporaryDivisionsPreviewsImageNames.Length - 1;
				string imageName = this._view.TemporaryDivisionsPreviewsImageNames[num];
				this._view.TemporaryPreviewImage.SetImageName(imageName);
				this._view.TemporaryPrizeListLabel.Text = this._translation.Get("RANKING_TOP_LEAGUE_REWARD_DESCRIPTION", TranslationContext.Ranked);
				ActivatableExtensions.Activate(this._view.TopPlacementRewardsObservation);
				return;
			}
			ActivatableExtensions.Deactivate(this._view.TopPlacementRewardsObservation);
			string imageName2 = this._view.TemporaryDivisionsPreviewsImageNames[this._currentDivisionIndex];
			this._view.TemporaryPreviewImage.SetImageName(imageName2);
			Division division = this._divisions[this._currentDivisionIndex];
			this._view.TemporaryPrizeListLabel.Text = this._translation.Get(string.Format("{0}_REWARD_DESCRIPTION", division.NameDraft), TranslationContext.Ranked);
		}

		private void UpdateDivisionDescription()
		{
			if (this.IsShowingTopPlacement())
			{
				this._view.DivisionNameLabel.Text = this._translation.Get("RANKING_HEAVYMETAL_LEAGUE", TranslationContext.Ranked);
				CompetitiveSeason competitiveSeason = this._getCurrentOrNextCompetitiveSeason.Get();
				this._view.DivisionScoreIntervalLabel.Text = this._translation.GetFormatted("RANKING_TOP_LEAGUE", TranslationContext.Ranked, new object[]
				{
					competitiveSeason.TopPlayersCount
				});
			}
			else
			{
				Division division = this._divisions[this._currentDivisionIndex];
				this._view.DivisionNameLabel.Text = this._translation.Get(division.NameDraft, TranslationContext.Ranked);
				this._view.DivisionScoreIntervalLabel.Text = string.Format("{0} - {1}", division.StartingScore, division.EndingScore);
			}
		}

		private void SelectFirstItem()
		{
			this._itemViews.First<ICompetitiveRewardItemView>().Toggle.IsOn = true;
		}

		private void AddDivisionItem()
		{
			ICompetitiveRewardItemView competitiveRewardItemView = this._view.CreateAndAddItem();
			string divisionThumbnailImage = this.GetDivisionThumbnailImage();
			competitiveRewardItemView.ThumbnailImage.SetImageName(divisionThumbnailImage);
			IDisposable value = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(competitiveRewardItemView.Toggle.OnToggleOn(), delegate(Unit _)
			{
				this.ChangePreviewToDivision();
			}));
			this._itemViews.Add(competitiveRewardItemView);
			this._itemViewsToggleSubscription[competitiveRewardItemView] = value;
		}

		private string GetDivisionThumbnailImage()
		{
			if (this.IsShowingTopPlacement())
			{
				return this._divisionsBadgeNameBuilder.GetTopDivisionBadgeFileName(400);
			}
			Division division = this._divisions[this._currentDivisionIndex];
			return this._divisionsBadgeNameBuilder.GetDivisionBadgeFileName(division, 100);
		}

		private void AddRewardsItems()
		{
			CompetitiveReward[] rewards = this.GetRewards();
			foreach (CompetitiveReward reward in rewards)
			{
				this.AddRewardItem(reward);
			}
		}

		private CompetitiveReward[] GetRewards()
		{
			if (this.IsShowingTopPlacement())
			{
				return this._getCompetitiveDivisionsPrizes.GetForTopPlayers();
			}
			return this._getCompetitiveDivisionsPrizes.GetForDivision(this._currentDivisionIndex);
		}

		private void AddRewardItem(CompetitiveReward reward)
		{
			ICompetitiveRewardItemView competitiveRewardItemView = this._view.CreateAndAddItem();
			competitiveRewardItemView.ThumbnailImage.SetImageName(reward.ThumbnailImageName);
			IDisposable value = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(competitiveRewardItemView.Toggle.OnToggleOn(), delegate(Unit _)
			{
				this.ChangePreviewToItem(reward);
			}));
			this._itemViews.Add(competitiveRewardItemView);
			this._itemViewsToggleSubscription[competitiveRewardItemView] = value;
		}

		private void ChangePreviewToDivision()
		{
			ActivatableExtensions.Deactivate(this._view.ItemPreviewerActivatable);
			this.DeactivateAllDivisionPreviews();
			ActivatableExtensions.Activate(this._view.DivisionsPreviews[this._currentDivisionIndex]);
		}

		private void ChangePreviewToItem(CompetitiveReward reward)
		{
			ActivatableExtensions.Activate(this._view.ItemPreviewerActivatable);
			this.DeactivateAllDivisionPreviews();
			this._view.ItemPreviewer.SetAsset(new UnityUiBattlepassArtPreview.ArtPreviewData
			{
				RewardAssetName = reward.AssetName,
				RewardAssetKind = reward.AssetKind,
				TitleText = reward.TitleDraft,
				DescriptionText = reward.DescriptionDraft
			});
		}

		private void DeactivateAllDivisionPreviews()
		{
			for (int i = 0; i < this._view.DivisionsPreviews.Length; i++)
			{
				ActivatableExtensions.Deactivate(this._view.DivisionsPreviews[i]);
			}
		}

		private void DestroyAllItems()
		{
			foreach (ICompetitiveRewardItemView competitiveRewardItemView in this._itemViews)
			{
				competitiveRewardItemView.Destroy();
				IDisposable disposable = this._itemViewsToggleSubscription[competitiveRewardItemView];
				disposable.Dispose();
			}
			this._itemViews.Clear();
			this._itemViewsToggleSubscription.Clear();
		}

		public IObservable<Unit> Show()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._view.BackButton.IsInteractable = false;
				return Observable.Do<Unit>(this._view.ShowAnimation.Play(), delegate(Unit _)
				{
					this._view.PreviousDivisionButton.IsInteractable = this.HasPreviousDivision();
					this._view.NextDivisionButton.IsInteractable = this.HasNextDivision();
					this._view.BackButton.IsInteractable = true;
					this._view.UiNavigationGroupHolder.AddGroup();
				});
			});
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._view.PreviousDivisionButton.IsInteractable = false;
				this._view.NextDivisionButton.IsInteractable = false;
				return Observable.Do<Unit>(Observable.Do<Unit>(this._view.HideAnimation.Play(), delegate(Unit _)
				{
					this._view.UiNavigationGroupHolder.RemoveGroup();
				}), delegate(Unit _)
				{
					this._hideSubject.OnNext(Unit.Default);
				});
			});
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this.DestroyAllItems();
			}), this._viewLoader.UnloadView("UI_ADD_RankingReward"));
		}

		public IObservable<Unit> ObserveHide()
		{
			return this._hideSubject;
		}

		private const string SceneName = "UI_ADD_RankingReward";

		private readonly IViewLoader _viewLoader;

		private readonly IViewProvider _viewProvider;

		private readonly ILocalizeKey _translation;

		private readonly IGetCurrentOrNextCompetitiveSeason _getCurrentOrNextCompetitiveSeason;

		private readonly IGetCompetitiveDivisions _getCompetitiveDivisions;

		private readonly IGetCompetitiveDivisionsPrizes _getCompetitiveDivisionsPrizes;

		private readonly Subject<Unit> _hideSubject = new Subject<Unit>();

		private readonly List<ICompetitiveRewardItemView> _itemViews = new List<ICompetitiveRewardItemView>();

		private readonly Dictionary<ICompetitiveRewardItemView, IDisposable> _itemViewsToggleSubscription = new Dictionary<ICompetitiveRewardItemView, IDisposable>();

		private readonly ICompetitiveDivisionsBadgeNameBuilder _divisionsBadgeNameBuilder;

		private readonly IMainMenuPresenterTree _mainMenuPresenterTree;

		private ICompetitiveRewardsView _view;

		private Division[] _divisions;

		private int _currentDivisionIndex;
	}
}
