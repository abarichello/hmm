using System;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.MatchMaking;
using HeavyMetalMachines.Matchmaking.Queue.Exceptions;
using HeavyMetalMachines.MatchMakingQueue.Infra;
using UniRx;

namespace HeavyMetalMachines.Matchmaking.Queue
{
	public class MatchmakingMatchSearch : IMatchmakingMatchSearch
	{
		public MatchmakingMatchSearch(IMatchmakingService matchmakingService, IMatchmakingStateStorage matchmakingStateStorage, ICurrentMatchStorage currentMatchStorage)
		{
			this._matchmakingService = matchmakingService;
			this._matchmakingStateStorage = matchmakingStateStorage;
			this._currentMatchStorage = currentMatchStorage;
		}

		public void Search(IMatchmakingMatchConfirmation matchConfirmation)
		{
			this._matchmakingMatchConfirmation = matchConfirmation;
			IMatchmakingStateStorage matchmakingStateStorage = this._matchmakingStateStorage;
			MatchmakingQueueState state = default(MatchmakingQueueState);
			state.Step = 1;
			matchmakingStateStorage.NotifyStateChanged(state);
			this._matchmakingStateStorage.CurrentSearch = ObservableExtensions.Subscribe<Unit>(Observable.DoOnCancel<Unit>(Observable.Catch<Unit, Exception>(Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				this.FindMatchAndWaitToStart(),
				this.ListenForErrors()
			}), new Func<Exception, IObservable<Unit>>(this.ResetStateAndNotifyError)), new Action(this.CancelSearch)));
		}

		private IObservable<Unit> FindMatchAndWaitToStart()
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(this.StartFindingMatch(), this.WaitUntilMatchIsFound()), this.WaitForPlayersToAcceptMatch()), this.WaitForMatchToStart());
		}

		private IObservable<Unit> ListenForErrors()
		{
			return Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				MatchmakingMatchSearch.ThrowExceptionOnNotification(this._matchmakingService.OnNoServerAvailable, "There is no server available."),
				MatchmakingMatchSearch.ThrowExceptionOnNotification(this._matchmakingService.OnMatchDisconnection, "You were disconnected from the match."),
				MatchmakingMatchSearch.ThrowExceptionOnNotification(this._matchmakingService.OnClientDisconnected, "You were disconnected."),
				MatchmakingMatchSearch.ThrowExceptionOnNotification(this._matchmakingService.OnMatchCanceled, "The match was canceled."),
				MatchmakingMatchSearch.ThrowExceptionOnNotification(this._matchmakingService.OnMatchError, "An error occurred in the match.")
			});
		}

		private static IObservable<Unit> ThrowExceptionOnNotification(IObservable<Unit> @event, string message)
		{
			return Observable.AsUnitObservable<Unit>(Observable.Do<Unit>(@event, delegate(Unit _)
			{
				throw new Exception(message);
			}));
		}

		private IObservable<Unit> ResetStateAndNotifyError(Exception error)
		{
			this.ResetState();
			this._matchmakingStateStorage.NotifyMatchmakingError(error);
			return Observable.ReturnUnit();
		}

		private void ResetState()
		{
			this._matchmakingStateStorage.CurrentSearch = null;
			IMatchmakingStateStorage matchmakingStateStorage = this._matchmakingStateStorage;
			MatchmakingQueueState state = default(MatchmakingQueueState);
			state.Step = 0;
			matchmakingStateStorage.NotifyStateChanged(state);
		}

		private void CancelSearch()
		{
			this._matchmakingService.CancelSearch(this._matchmakingService.GetCurrentQueueName());
			this.ResetState();
		}

		private IObservable<Unit> StartFindingMatch()
		{
			return Observable.AsUnitObservable<MatchmakingStartMatchSearchResult>(Observable.Do<MatchmakingStartMatchSearchResult>(this._matchmakingService.StartFindingMatch(0), new Action<MatchmakingStartMatchSearchResult>(this.ChangeStepToFindingMatch)));
		}

		private IObservable<Unit> WaitUntilMatchIsFound()
		{
			return Observable.Do<Unit>(Observable.First<Unit>(this._matchmakingService.OnMatchFound), delegate(Unit _)
			{
				this.ChangeStepToMatchFound();
			});
		}

		private void ChangeStepToFindingMatch(MatchmakingStartMatchSearchResult matchmakingStartMatchSearchResult)
		{
			IMatchmakingStateStorage matchmakingStateStorage = this._matchmakingStateStorage;
			MatchmakingQueueState state = default(MatchmakingQueueState);
			state.Step = 2;
			state.EstimatedWaitTimeMinutes = matchmakingStartMatchSearchResult.EstimatedWaitTimeMinutes;
			matchmakingStateStorage.NotifyStateChanged(state);
		}

		private void ChangeStepToMatchFound()
		{
			IMatchmakingStateStorage matchmakingStateStorage = this._matchmakingStateStorage;
			MatchmakingQueueState state = default(MatchmakingQueueState);
			state.Step = 3;
			state.AcceptMatchTimeoutSeconds = 20L;
			matchmakingStateStorage.NotifyStateChanged(state);
			IMatchmakingStateStorage matchmakingStateStorage2 = this._matchmakingStateStorage;
			MatchmakingQueueState state2 = default(MatchmakingQueueState);
			state2.Step = 4;
			matchmakingStateStorage2.NotifyStateChanged(state2);
		}

		private IObservable<Unit> WaitForPlayersToAcceptMatch()
		{
			return Observable.AsUnitObservable<int>(Observable.Do<int>(Observable.Where<int>(Observable.Do<int>(Observable.Scan<Unit, int>(Observable.Take<Unit>(Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				this.WaitPlayerToAcceptMatch(),
				this.WaitOtherPlayersToAcceptMatch(),
				this.ThrowErrorWhenOtherPlayersDeclineMatch()
			}), 8), 0, (int numberOfPlayers, Unit _) => numberOfPlayers + 1), new Action<int>(this.NotifyPlayerAccepted)), (int numberOfPlayers) => numberOfPlayers >= 8), new Action<int>(this.ChangeStepToWaitingServiceToStartMatch)));
		}

		private void NotifyPlayerAccepted(int numberOfPlayers)
		{
			IMatchmakingStateStorage matchmakingStateStorage = this._matchmakingStateStorage;
			MatchmakingQueueState state = default(MatchmakingQueueState);
			state.Step = 4;
			state.NumberOfPlayersThatAcceptedMatch = numberOfPlayers;
			matchmakingStateStorage.NotifyStateChanged(state);
		}

		private void ChangeStepToWaitingServiceToStartMatch(int numberOfPlayers)
		{
			IMatchmakingStateStorage matchmakingStateStorage = this._matchmakingStateStorage;
			MatchmakingQueueState state = default(MatchmakingQueueState);
			state.Step = 5;
			matchmakingStateStorage.NotifyStateChanged(state);
		}

		private IObservable<Unit> WaitOtherPlayersToAcceptMatch()
		{
			return Observable.AsUnitObservable<string>(Observable.Distinct<string>(this._matchmakingService.OnPlayerAcceptedMatch));
		}

		private IObservable<Unit> ThrowErrorWhenOtherPlayersDeclineMatch()
		{
			return Observable.AsUnitObservable<string>(Observable.Do<string>(this._matchmakingService.OnPlayerDeclinedMatch, delegate(string playerId)
			{
				throw new PlayerDeclinedMatchException(playerId);
			}));
		}

		private IObservable<Unit> WaitPlayerToAcceptMatch()
		{
			return Observable.AsUnitObservable<bool>(Observable.Do<bool>(this._matchmakingMatchConfirmation.ConfirmMatch(), new Action<bool>(this.HandlePlayerMatchConfirmation)));
		}

		private void HandlePlayerMatchConfirmation(bool wasAccepted)
		{
			if (wasAccepted)
			{
				this._matchmakingService.AcceptMatch();
				return;
			}
			this._matchmakingService.DeclineMatch();
			throw new PlayerDeclinedMatchException(string.Empty);
		}

		private IObservable<Unit> WaitForMatchToStart()
		{
			return Observable.AsUnitObservable<Match>(Observable.Do<Match>(Observable.Do<Match>(Observable.First<Match>(this._matchmakingService.OnMatchReady), new Action<Match>(this.FillCurrentMatchStorage)), new Action<Match>(this.GetMatchStartedAndNoneStates)));
		}

		private void FillCurrentMatchStorage(Match match)
		{
			this._currentMatchStorage.CurrentMatch = new Match?(match);
		}

		private void GetMatchStartedAndNoneStates(Match match)
		{
			IMatchmakingStateStorage matchmakingStateStorage = this._matchmakingStateStorage;
			MatchmakingQueueState state = default(MatchmakingQueueState);
			state.Match = match;
			state.Step = 6;
			matchmakingStateStorage.NotifyStateChanged(state);
			this.ResetState();
		}

		private const int TotalNumberOfPlayersInMatch = 8;

		private readonly IMatchmakingService _matchmakingService;

		private readonly IMatchmakingStateStorage _matchmakingStateStorage;

		private readonly ICurrentMatchStorage _currentMatchStorage;

		private IMatchmakingMatchConfirmation _matchmakingMatchConfirmation;
	}
}
