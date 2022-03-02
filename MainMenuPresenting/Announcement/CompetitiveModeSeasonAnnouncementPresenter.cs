using System;
using HeavyMetalMachines.CompetitiveMode.Seasons;
using HeavyMetalMachines.Presenting;
using UniRx;

namespace HeavyMetalMachines.MainMenuPresenting.Announcement
{
	public class CompetitiveModeSeasonAnnouncementPresenter
	{
		public CompetitiveModeSeasonAnnouncementPresenter(IViewProvider viewProvider, IGetCurrentOrNextCompetitiveSeason getCurrentOrNextCompetitiveSeason)
		{
			this._viewProvider = viewProvider;
			this._getCurrentOrNextCompetitiveSeason = getCurrentOrNextCompetitiveSeason;
		}

		public IObservable<Unit> OnOpenWindowButtonClick
		{
			get
			{
				return this._openWindowbuttonClick;
			}
		}

		public void Initialize(bool showAnnouncement)
		{
			this._view = this._viewProvider.Provide<UnityCompetitiveModeSeasonAnnouncementView>(null);
			this.SetupView();
			this._view.Initialize(showAnnouncement);
		}

		public void SetOpenWindowButtonInteractable(bool interactable)
		{
			this._view.OpenWindowButton.IsInteractable = interactable;
		}

		private void SetupView()
		{
			CompetitiveSeason competitiveSeason = this._getCurrentOrNextCompetitiveSeason.Get();
			this._view.OpenWindowButton.IsInteractable = false;
			this._view.StartDayLabel.Text = competitiveSeason.StartDateTime.Day.ToString("0");
			this._view.StartMonthLabel.Text = string.Format("/{0}", competitiveSeason.StartDateTime.Month);
			this._view.EndDayLabel.Text = competitiveSeason.EndDateTime.Day.ToString("0");
			this._view.EndMonthLabel.Text = competitiveSeason.EndDateTime.Month.ToString("/0");
			ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._view.OpenWindowButton.OnClick(), new Action<Unit>(this._openWindowbuttonClick.OnNext)));
		}

		private readonly IViewProvider _viewProvider;

		private UnityCompetitiveModeSeasonAnnouncementView _view;

		private readonly IGetCurrentOrNextCompetitiveSeason _getCurrentOrNextCompetitiveSeason;

		private readonly Subject<Unit> _openWindowbuttonClick = new Subject<Unit>();
	}
}
