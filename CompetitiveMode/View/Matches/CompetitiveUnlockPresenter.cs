using System;
using HeavyMetalMachines.CompetitiveMode.Matchmaking;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.Presenting;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class CompetitiveUnlockPresenter : ICompetitiveUnlockPresenter
	{
		public CompetitiveUnlockPresenter(IWaitAndGetMyPlayerCompetitiveStateProgress waitAndGetMyPlayerCompetitiveStateProgress, IViewProvider viewProvider)
		{
			this._waitAndGetMyPlayerCompetitiveStateProgress = waitAndGetMyPlayerCompetitiveStateProgress;
			this._viewProvider = viewProvider;
		}

		public IObservable<Unit> Show()
		{
			return Observable.ContinueWith<PlayerCompetitiveProgress, Unit>(Observable.Do<PlayerCompetitiveProgress>(this._waitAndGetMyPlayerCompetitiveStateProgress.WaitAndGet(), delegate(PlayerCompetitiveProgress progress)
			{
				this.InitializeView(progress.FinalState);
			}), new Func<PlayerCompetitiveProgress, IObservable<Unit>>(this.AnnounceCompetitiveModeUnlockIfNeeded));
		}

		private void InitializeView(PlayerCompetitiveState playerCompetitiveState)
		{
			this._view = this._viewProvider.Provide<ICompetitiveUnlockView>(null);
			this._announcementView = this._viewProvider.Provide<ICompetitiveUnlockAnnouncementView>(null);
			if (playerCompetitiveState.Status != null)
			{
				ActivatableExtensions.Deactivate(this._view.UnlockMatchesGroup);
				return;
			}
			ActivatableExtensions.Activate(this._view.UnlockMatchesGroup);
			int totalMatchesPlayed = playerCompetitiveState.Requirements.TotalMatchesPlayed;
			this._view.MatchesPlayedLabel.Text = string.Format("{0} /", totalMatchesPlayed.ToString());
			this._view.TotalMatchesNeededLabel.Text = playerCompetitiveState.Requirements.TotalRequiredMatches.ToString();
		}

		private IObservable<Unit> AnnounceCompetitiveModeUnlockIfNeeded(PlayerCompetitiveProgress progress)
		{
			if (CompetitiveUnlockPresenter.HasUnlockedCompetitiveMode(progress))
			{
				return this.ShowCompetitiveModeUnlock();
			}
			return Observable.ReturnUnit();
		}

		private static bool HasUnlockedCompetitiveMode(PlayerCompetitiveProgress progress)
		{
			return progress.InitialState.Status == null && progress.FinalState.Status != 0;
		}

		private IObservable<Unit> ShowCompetitiveModeUnlock()
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				ActivatableExtensions.Activate(this._announcementView.Group);
			}), (Unit _) => this.PlayShowAnimation()), delegate(Unit _)
			{
				ObservableExtensions.Subscribe<Unit>(this._announcementView.IdleAnimation.Play());
			}), (Unit _) => this._announcementView.ShowConfirmButtonAnimation.Play()), delegate(Unit _)
			{
				this._announcementView.UiNavigationGroupHolder.AddGroup();
			}), (Unit _) => Observable.First<Unit>(this._announcementView.ConfirmButton.OnClick())), delegate(Unit _)
			{
				this._announcementView.UiNavigationGroupHolder.RemoveGroup();
			}), (Unit _) => this.PlayHideAnimation());
		}

		private IObservable<Unit> PlayShowAnimation()
		{
			return Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				this._announcementView.ParentShowAnimation.Play(),
				this._announcementView.ShowAnimation.Play()
			});
		}

		private IObservable<Unit> PlayHideAnimation()
		{
			return Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				this._announcementView.ParentHideAnimation.Play(),
				this._announcementView.HideAnimation.Play()
			});
		}

		private readonly IWaitAndGetMyPlayerCompetitiveStateProgress _waitAndGetMyPlayerCompetitiveStateProgress;

		private readonly IViewProvider _viewProvider;

		private ICompetitiveUnlockView _view;

		private ICompetitiveUnlockAnnouncementView _announcementView;
	}
}
