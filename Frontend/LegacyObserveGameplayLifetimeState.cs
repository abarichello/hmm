using System;
using HeavyMetalMachines.Gameplay;
using Pocketverse;
using UniRx;

namespace HeavyMetalMachines.Frontend
{
	public class LegacyObserveGameplayLifetimeState : IObserveGameplayLifetimeState
	{
		public LegacyObserveGameplayLifetimeState(LoadingVersusController loadingVersusController, StateMachine stateMachine, GuiLoadingController loadingController)
		{
			this._loadingVersusController = loadingVersusController;
			this._stateMachine = stateMachine;
			this._loadingController = loadingController;
		}

		public GameplayLifetimeState Get()
		{
			if (this._loadingVersusController.IsLoading)
			{
				return 1;
			}
			if (this._stateMachine.CurrentStateKind == GameState.GameStateKind.MatchMaking && this._loadingController.IsLoading)
			{
				return 2;
			}
			return 0;
		}

		public IObservable<GameplayLifetimeState> Observe()
		{
			return Observable.Select<Unit, GameplayLifetimeState>(Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				this._loadingVersusController.OnLoadingStarted,
				this._loadingVersusController.OnLoadingFinished,
				this.StateMachineStateChanged()
			}), (Unit _) => this.Get());
		}

		private IObservable<Unit> StateMachineStateChanged()
		{
			return Observable.AsUnitObservable<GameState.GameStateKind>(this._stateMachine.StateChangedObservation());
		}

		private readonly LoadingVersusController _loadingVersusController;

		private readonly StateMachine _stateMachine;

		private readonly GuiLoadingController _loadingController;
	}
}
