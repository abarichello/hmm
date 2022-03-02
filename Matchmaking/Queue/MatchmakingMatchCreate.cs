using System;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.MatchMaking;
using HeavyMetalMachines.MatchMakingQueue.Infra;
using UniRx;

namespace HeavyMetalMachines.Matchmaking.Queue
{
	public class MatchmakingMatchCreate : IMatchmakingMatchCreate
	{
		public MatchmakingMatchCreate(IMatchmakingService matchmakingService, IMatchmakingStateStorage matchmakingStateStorage, ICurrentMatchStorage currentMatchStorage)
		{
			this._matchmakingService = matchmakingService;
			this._matchmakingStateStorage = matchmakingStateStorage;
			this._currentMatchStorage = currentMatchStorage;
		}

		public void CreateMatch(string config)
		{
			this.ChangeStepToWaitingServiceToStartMatch();
			this._matchmakingStateStorage.CurrentSearch = ObservableExtensions.Subscribe<Unit>(Observable.DoOnCancel<Unit>(Observable.Catch<Unit, Exception>(Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				this.CreateAndWaitForMatchToStart(config),
				this.ListenForErrors()
			}), new Func<Exception, IObservable<Unit>>(this.ResetStateAndNotifyError)), new Action(this.CancelSearch)));
		}

		private IObservable<Unit> ListenForErrors()
		{
			return Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				MatchmakingMatchCreate.ThrowExceptionOnNotification(this._matchmakingService.OnNoServerAvailable, "There is no server available."),
				MatchmakingMatchCreate.ThrowExceptionOnNotification(this._matchmakingService.OnMatchDisconnection, "You were disconnected from the match."),
				MatchmakingMatchCreate.ThrowExceptionOnNotification(this._matchmakingService.OnClientDisconnected, "You were disconnected."),
				MatchmakingMatchCreate.ThrowExceptionOnNotification(this._matchmakingService.OnMatchCanceled, "The match was canceled."),
				MatchmakingMatchCreate.ThrowExceptionOnNotification(this._matchmakingService.OnMatchError, "An error occurred in the match."),
				MatchmakingMatchCreate.ThrowExceptionOnNotification(this._matchmakingService.OnConnectionInstability, "An instability connection occurs.")
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

		private void ChangeStepToWaitingServiceToStartMatch()
		{
			IMatchmakingStateStorage matchmakingStateStorage = this._matchmakingStateStorage;
			MatchmakingQueueState state = default(MatchmakingQueueState);
			state.Step = 5;
			matchmakingStateStorage.NotifyStateChanged(state);
		}

		private IObservable<Unit> CreateAndWaitForMatchToStart(string config)
		{
			return Observable.AsUnitObservable<Match>(Observable.Do<Match>(Observable.Do<Match>(Observable.First<Match>(this._matchmakingService.CreateMatch(config)), new Action<Match>(this.FillCurrentMatchStorage)), new Action<Match>(this.GetMatchStartedAndNoneStates)));
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

		private readonly IMatchmakingService _matchmakingService;

		private readonly IMatchmakingStateStorage _matchmakingStateStorage;

		private readonly ICurrentMatchStorage _currentMatchStorage;
	}
}
