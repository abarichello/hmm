using System;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.MatchMaking;
using HeavyMetalMachines.Matchmaking.Queue.Exceptions;
using HeavyMetalMachines.Presenting;
using Hoplon.Input;
using Hoplon.Localization.TranslationTable;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public class MatchmakingDialogsPresenter : IMatchmakingDialogsPresenter
	{
		public MatchmakingDialogsPresenter(IGetThenObserveMatchmakingQueueState getThenObserveMatchmakingQueueState, ICancelMatchmakingMatchSearch cancelMatchmakingMatchSearch, IViewProvider viewProvider, ILocalizeKey translation, IInputActiveDeviceChangeNotifier activeDeviceChangeNotifier)
		{
			this._getThenObserveMatchmakingQueueState = getThenObserveMatchmakingQueueState;
			this._cancelMatchmakingMatchSearch = cancelMatchmakingMatchSearch;
			this._viewProvider = viewProvider;
			this._translation = translation;
			this._activeDeviceChangeNotifier = activeDeviceChangeNotifier;
		}

		public void Initialize()
		{
			this._findingMatchView = this._viewProvider.Provide<IMatchmakingFindingMatchView>(null);
			this._matchAcceptView = this._viewProvider.Provide<IMatchmakingMatchAcceptView>(null);
			ObservableExtensions.Subscribe<MatchmakingQueueState>(this._getThenObserveMatchmakingQueueState.GetThenObserve(), new Action<MatchmakingQueueState>(this.ManageMatchmakingState));
			ObservableExtensions.Subscribe<PlayerDeclinedMatchException>(Observable.Do<PlayerDeclinedMatchException>(Observable.OfType<Exception, PlayerDeclinedMatchException>(Observable.Do<Exception>(this._getThenObserveMatchmakingQueueState.ObserveErrors(), delegate(Exception _)
			{
				this.HideAcceptDeclineButtons();
				this._acceptMatchTimeoutCounting.Dispose();
			})), delegate(PlayerDeclinedMatchException _)
			{
				ActivatableExtensions.Activate(this._matchAcceptView.PlayerDeclinedMessage);
				this._matchAcceptView.SetPlayerIndexAsDeclined(this._acceptedPlayerCount);
			}));
			ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._findingMatchView.CancelSearchButton.OnClick(), delegate(Unit _)
			{
				this._cancelMatchmakingMatchSearch.Cancel();
				this._elapsedTimeCounting.Dispose();
			}));
			ObservableExtensions.Subscribe<InputDevice>(Observable.Do<InputDevice>(this._activeDeviceChangeNotifier.GetAndObserveActiveDeviceChange(), delegate(InputDevice device)
			{
				this.SetActiveCancelSearchButton(device != 3);
			}));
			this._acceptedPlayerCount = 0;
			this.ShowAcceptDeclineButtons();
		}

		private void ManageMatchmakingState(MatchmakingQueueState state)
		{
			string str = state.Step.ToString();
			if (state.Step == 4)
			{
				str += string.Format("({0} players accepted)", state.NumberOfPlayersThatAcceptedMatch);
			}
			switch (state.Step)
			{
			case 0:
				this.HideBothViews();
				break;
			case 2:
				this.ShowFindingMatchView(state.EstimatedWaitTimeMinutes);
				break;
			case 3:
				this.ShowAcceptMatchView(state.AcceptMatchTimeoutSeconds);
				break;
			case 4:
				this.UpdateAcceptMatchView(state.NumberOfPlayersThatAcceptedMatch);
				break;
			}
		}

		private void ShowAcceptDeclineButtons()
		{
			ActivatableExtensions.Activate(this._matchAcceptView.AcceptButtonParent);
			ActivatableExtensions.Activate(this._matchAcceptView.DeclineButtonParent);
		}

		private void HideAcceptDeclineButtons()
		{
			ActivatableExtensions.Deactivate(this._matchAcceptView.AcceptButtonParent);
			ActivatableExtensions.Deactivate(this._matchAcceptView.DeclineButtonParent);
		}

		private void ShowFindingMatchView(long estimatedWaitTimeMinutes)
		{
			this._matchAcceptView.DeactivateAllPlayerIndicators();
			this.InitializeAverageWaitTime(estimatedWaitTimeMinutes);
			this.StartCountingElapsedTime();
			ObservableExtensions.Subscribe<Unit>(this._findingMatchView.ShowAnimation.Play());
		}

		private void SetActiveCancelSearchButton(bool active)
		{
			Debug.LogError(active);
			this._findingMatchView.CancelSearchButton.SetActive(active);
		}

		private void UpdateAcceptMatchView(int acceptedPlayerCount)
		{
			if (acceptedPlayerCount <= 0)
			{
				return;
			}
			this._acceptedPlayerCount = acceptedPlayerCount;
			this._matchAcceptView.SetPlayerIndexAsAccepted(acceptedPlayerCount - 1);
		}

		private void ShowAcceptMatchView(long timeoutSeconds)
		{
			this._elapsedTimeCounting.Dispose();
			ActivatableExtensions.Activate(this._matchAcceptView.MainGroup);
			ObservableExtensions.Subscribe<Unit>(this._matchAcceptView.ShowAnimation.Play());
			this.ShowAcceptDeclineButtons();
			this.SetCurrentAcceptMatchTimeoutTime(0L, timeoutSeconds);
			this._acceptMatchTimeoutCounting = ObservableExtensions.Subscribe<long>(Observable.Do<long>(Observable.Select<long, long>(Observable.Take<long>(Observable.Interval(TimeSpan.FromSeconds(1.0)), (int)timeoutSeconds), (long intervalIndex) => intervalIndex + 1L), delegate(long secondsPassed)
			{
				this.SetCurrentAcceptMatchTimeoutTime(secondsPassed, timeoutSeconds);
			}));
		}

		private void SetCurrentAcceptMatchTimeoutTime(long secondsPassed, long timeoutSeconds)
		{
			long num = timeoutSeconds - secondsPassed;
			this._matchAcceptView.TimeoutProgressBar.FillPercent = (float)num / (float)timeoutSeconds;
			TimeSpan timeSpan = TimeSpan.FromSeconds((double)num);
			this._matchAcceptView.TimeoutLabel.Text = string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
		}

		private void HideBothViews()
		{
			ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._matchAcceptView.HideAnimation.Play(), delegate(Unit _)
			{
				ActivatableExtensions.Deactivate(this._matchAcceptView.MainGroup);
			}));
			ObservableExtensions.Subscribe<Unit>(this._findingMatchView.HideAnimation.Play());
		}

		private void InitializeAverageWaitTime(long estimatedWaitTimeMinutes)
		{
			string arg = this._translation.Get("QUEUE_AVG_TIME", TranslationContext.MainMenuGui);
			string arg2;
			if (estimatedWaitTimeMinutes > 10L)
			{
				arg2 = "+10";
			}
			else
			{
				estimatedWaitTimeMinutes = Math.Max(estimatedWaitTimeMinutes, 1L);
				arg2 = estimatedWaitTimeMinutes.ToString();
			}
			this._findingMatchView.AverageWaitTimeLabel.Text = string.Format("{0} {1}", arg2, arg);
		}

		private void StartCountingElapsedTime()
		{
			this.SetCurrentWaitTime(0L);
			this._elapsedTimeCounting = ObservableExtensions.Subscribe<long>(Observable.Do<long>(Observable.Select<long, long>(Observable.Interval(TimeSpan.FromSeconds(1.0)), (long intervalIndex) => intervalIndex + 1L), new Action<long>(this.SetCurrentWaitTime)));
		}

		private void SetCurrentWaitTime(long secondsPassed)
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds((double)secondsPassed);
			string text = string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
			this._findingMatchView.ElapsedWaitTimeLabel.Text = text;
		}

		private readonly IGetThenObserveMatchmakingQueueState _getThenObserveMatchmakingQueueState;

		private readonly ICancelMatchmakingMatchSearch _cancelMatchmakingMatchSearch;

		private readonly IViewProvider _viewProvider;

		private readonly ILocalizeKey _translation;

		private readonly IInputActiveDeviceChangeNotifier _activeDeviceChangeNotifier;

		private IMatchmakingFindingMatchView _findingMatchView;

		private IMatchmakingMatchAcceptView _matchAcceptView;

		private IDisposable _elapsedTimeCounting;

		private IDisposable _acceptMatchTimeoutCounting;

		private int _acceptedPlayerCount;
	}
}
