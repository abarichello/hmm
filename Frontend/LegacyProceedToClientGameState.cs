using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting;
using HeavyMetalMachines.Match;
using Pocketverse;
using UniRx;

namespace HeavyMetalMachines.Frontend
{
	public class LegacyProceedToClientGameState : IProceedToClientGameState
	{
		public LegacyProceedToClientGameState(HMMHub hub, StateMachine stateMachine, LoadingState loadingState)
		{
			this._hub = hub;
			this._stateMachine = stateMachine;
			this._loadingState = loadingState;
		}

		public IObservable<Unit> Proceed()
		{
			return Observable.AsUnitObservable<long>(Observable.Do<long>(Observable.First<long>(Observable.EveryUpdate(), (long _) => this.IsServerReady()), delegate(long _)
			{
				this.GoToLoadingState();
			}));
		}

		private bool IsServerReady()
		{
			return this._hub.Match.State == MatchData.MatchState.MatchStarted || this._hub.Match.State == MatchData.MatchState.PreMatch;
		}

		private void GoToLoadingState()
		{
			this._stateMachine.GotoState(this._loadingState, false);
		}

		private readonly HMMHub _hub;

		private readonly StateMachine _stateMachine;

		private readonly LoadingState _loadingState;
	}
}
