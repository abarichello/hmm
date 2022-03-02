using System;
using Pocketverse;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Welcome
{
	public class WelcomeGameStateBehaviour : GameState
	{
		protected override IObservable<Unit> OnStateEnabledAsync()
		{
			IExecuteWelcomeState executeWelcomeState = this._diContainer.Resolve<IExecuteWelcomeState>();
			return Observable.DoOnCompleted<Unit>(executeWelcomeState.Initialize(), delegate()
			{
				this.ExecuteState(executeWelcomeState);
			});
		}

		private void ExecuteState(IExecuteWelcomeState executeWelcomeState)
		{
			this._disposable = ObservableExtensions.Subscribe<Unit>(executeWelcomeState.Execute());
		}

		protected override void OnStateDisabled()
		{
			this._disposable.Dispose();
			base.OnStateDisabled();
		}

		[Inject]
		private DiContainer _diContainer;

		private IDisposable _disposable;
	}
}
