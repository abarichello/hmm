using System;
using System.Collections.Generic;
using System.Linq;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.Matchmaking.Configuration;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Extensions;
using HeavyMetalMachines.Regions.Business;
using Hoplon.Localization;
using Hoplon.Localization.TranslationTable;
using Hoplon.Time;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.View.Matchmaking
{
	public class CompetitiveQueuePeriodsPresenter : ICompetitiveQueuePeriodsPresenter, IPresenter
	{
		public CompetitiveQueuePeriodsPresenter(IViewLoader viewLoader, IViewProvider viewProvider, IGetCompetitiveQueueConfiguration getCompetitiveQueueConfiguration, ICurrentTime currentTime, ILocalizeDateTime localizeDateTime, ILocalizeKey translation, IMainMenuPresenterTree mainMenuPresenterTree, IGetRegions getRegions)
		{
			this._viewLoader = viewLoader;
			this._viewProvider = viewProvider;
			this._getCompetitiveQueueConfiguration = getCompetitiveQueueConfiguration;
			this._currentTime = currentTime;
			this._localizeDateTime = localizeDateTime;
			this._translation = translation;
			this._mainMenuPresenterTree = mainMenuPresenterTree;
			this._getRegions = getRegions;
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.ContinueWith<Unit, Unit>(this._viewLoader.LoadView("UI_ADD_RankingSchedule"), (Unit _) => this.InitializeView());
		}

		private IObservable<Unit> InitializeView()
		{
			this._view = this._viewProvider.Provide<ICompetitiveQueuePeriodsView>(null);
			ObservableExtensions.Subscribe<Unit>(ButtonExtensions.OnClickNavigateBackwards(this._view.BackButton, this._mainMenuPresenterTree.PresenterTree));
			Region selected = this._getRegions.GetSelected();
			this.InitializeRegionsDropdown(selected);
			return this.UpdateQueueDays(selected.Name);
		}

		private void InitializeRegionsDropdown(Region currentRegion)
		{
			Region[] all = this._getRegions.GetAll();
			List<string> list = (from region in all
			select region.Name).ToList<string>();
			List<string> list2 = (from region in all
			select this._translation.Get(region.NameDraft, TranslationContext.Region)).ToList<string>();
			this._view.RegionDropdown.ClearOptions();
			this._view.RegionDropdown.AddOptions(list, list2);
			this._view.RegionDropdown.SelectedOption = currentRegion.Name;
			ObservableExtensions.Subscribe<Unit>(Observable.SelectMany<string, Unit>(this._view.RegionDropdown.OnSelectionChanged(), (string regionName) => Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(this._view.ValuesChangeOutAnimation.Play(), (Unit _) => this.UpdateQueueDays(regionName)), this._view.ValuesChangeInAnimation.Play())));
		}

		private IObservable<Unit> UpdateQueueDays(string regionName)
		{
			return Observable.AsUnitObservable<QueueConfiguration>(Observable.Do<QueueConfiguration>(this._getCompetitiveQueueConfiguration.GetForRegion(regionName), new Action<QueueConfiguration>(this.InitializeQueueDays)));
		}

		private void InitializeQueueDays(QueueConfiguration queueConfiguration)
		{
			ICompetitiveQueueDayView[] array = this._view.QueueDays.ToArray<ICompetitiveQueueDayView>();
			DateTime date = this._currentTime.Now().Date;
			for (int i = 0; i < 7; i++)
			{
				DateTime dayOfWeek = date.AddDays((double)i);
				ICompetitiveQueueDayView dayView = array[i];
				this.InitializeQueueDayView(queueConfiguration, dayView, dayOfWeek);
			}
		}

		private void InitializeQueueDayView(QueueConfiguration queueConfiguration, ICompetitiveQueueDayView dayView, DateTime dayOfWeek)
		{
			dayView.ClearPeriods();
			dayView.DateLabel.Text = LocalizationExtensions.GetShortDateString(this._localizeDateTime, dayOfWeek);
			dayView.DayOfWeekLabel.Text = LocalizeKeyExtensions.GetDayOfWeek(this._translation, dayOfWeek.DayOfWeek);
			IEnumerable<QueuePeriod> enumerable = from p in queueConfiguration.QueuePeriods
			where p.OpenDateTimeUtc.ToLocalTime().Date == dayOfWeek
			select p;
			if (!enumerable.Any<QueuePeriod>())
			{
				ActivatableExtensions.Activate(dayView.EmptyPeriodsIndicator);
				dayView.BackgroundImage.Color = dayView.EmptyBackgroundColor;
				dayView.DateLabel.Color = dayView.EmptyDateColor;
				dayView.DayOfWeekLabel.Color = dayView.EmptyDayOfWeekColor;
				return;
			}
			ActivatableExtensions.Deactivate(dayView.EmptyPeriodsIndicator);
			dayView.BackgroundImage.Color = dayView.ActiveBackgroundColor;
			dayView.DateLabel.Color = dayView.ActiveDateColor;
			dayView.DayOfWeekLabel.Color = dayView.ActiveDayOfWeekColor;
			foreach (QueuePeriod queuePeriod in enumerable)
			{
				ICompetitiveQueueDayPeriodView competitiveQueueDayPeriodView = dayView.CreateAndAddPeriod();
				competitiveQueueDayPeriodView.OpenTimeLabel.Text = LocalizationExtensions.GetShortTimeString(this._localizeDateTime, queuePeriod.OpenDateTimeUtc.ToLocalTime());
				competitiveQueueDayPeriodView.CloseTimeLabel.Text = LocalizationExtensions.GetShortTimeString(this._localizeDateTime, queuePeriod.CloseDateTimeUtc.ToLocalTime());
			}
		}

		public IObservable<Unit> Show()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._view.MainCanvas.Enable();
				this._view.BackButton.IsInteractable = false;
				return Observable.Do<Unit>(Observable.Do<Unit>(this._view.ShowAnimation.Play(), delegate(Unit _)
				{
					this._view.BackButton.IsInteractable = true;
				}), delegate(Unit _)
				{
					this._view.UiNavigationGroupHolder.AddGroup();
				});
			});
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Defer<Unit>(() => Observable.Do<Unit>(Observable.Do<Unit>(this._view.HideAnimation.Play(), delegate(Unit _)
			{
				this._view.UiNavigationGroupHolder.RemoveGroup();
			}), delegate(Unit _)
			{
				this._hideSubject.OnNext(Unit.Default);
			}));
		}

		public IObservable<Unit> Dispose()
		{
			return this._viewLoader.UnloadView("UI_ADD_RankingSchedule");
		}

		public IObservable<Unit> ObserveHide()
		{
			return this._hideSubject;
		}

		private const string SceneName = "UI_ADD_RankingSchedule";

		private readonly IViewLoader _viewLoader;

		private readonly IViewProvider _viewProvider;

		private readonly IGetCompetitiveQueueConfiguration _getCompetitiveQueueConfiguration;

		private readonly ICurrentTime _currentTime;

		private readonly ILocalizeDateTime _localizeDateTime;

		private readonly ILocalizeKey _translation;

		private readonly IMainMenuPresenterTree _mainMenuPresenterTree;

		private readonly IGetRegions _getRegions;

		private readonly IGetThenObserveChosenRegionChanged _getThenObserveChosenRegionChanged;

		private readonly Subject<Unit> _hideSubject = new Subject<Unit>();

		private ICompetitiveQueuePeriodsView _view;
	}
}
