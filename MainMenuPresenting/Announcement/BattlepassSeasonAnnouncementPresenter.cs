using System;
using HeavyMetalMachines.Battlepass;
using HeavyMetalMachines.Battlepass.Business;
using HeavyMetalMachines.Presenting;
using UniRx;

namespace HeavyMetalMachines.MainMenuPresenting.Announcement
{
	public class BattlepassSeasonAnnouncementPresenter
	{
		public BattlepassSeasonAnnouncementPresenter(IViewProvider viewProvider, BattlepassSeason battlepassSeason, IGetBattlepassDateTranslation getBattlepassDateTranslation)
		{
			this._viewProvider = viewProvider;
			this._battlepassSeason = battlepassSeason;
			this._getBattlepassDateTranslation = getBattlepassDateTranslation;
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
			this._view = this._viewProvider.Provide<UnityBattlepassSeasonAnnouncementView>(null);
			this.SetupView();
			this._view.Initialize(showAnnouncement);
		}

		public void SetOpenWindowButtonInteractable(bool interactable)
		{
			this._view.OpenWindowButton.IsInteractable = interactable;
		}

		private void SetupView()
		{
			DateTime startSeasonDateTime = this._battlepassSeason.StartSeasonDateTime;
			DateTime endSeasonDateTime = this._battlepassSeason.EndSeasonDateTime;
			string month = this._getBattlepassDateTranslation.GetMonth(startSeasonDateTime);
			string month2 = this._getBattlepassDateTranslation.GetMonth(endSeasonDateTime);
			this._view.OpenWindowButton.IsInteractable = false;
			this._view.StartDayLabel.Text = startSeasonDateTime.Day.ToString("0");
			this._view.StartMonthLabel.Text = string.Format("/{0}", month);
			this._view.EndDayLabel.Text = endSeasonDateTime.Day.ToString("0");
			this._view.EndMonthLabel.Text = string.Format("/{0}", month2);
			ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._view.OpenWindowButton.OnClick(), new Action<Unit>(this._openWindowbuttonClick.OnNext)));
		}

		private readonly IViewProvider _viewProvider;

		private readonly IGetBattlepassDateTranslation _getBattlepassDateTranslation;

		private UnityBattlepassSeasonAnnouncementView _view;

		private BattlepassSeason _battlepassSeason;

		private readonly Subject<Unit> _openWindowbuttonClick = new Subject<Unit>();
	}
}
