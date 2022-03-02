using System;
using HeavyMetalMachines.ApplicationStates;
using HeavyMetalMachines.Login;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.Social.Profile.Business;
using HeavyMetalMachines.Tutorials;
using Hoplon;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.Welcome
{
	public class ExecuteWelcomeState : IExecuteWelcomeState
	{
		public ExecuteWelcomeState(IWelcomePresenter welcomePresenter, IProceedToMainMenu proceedToMainMenu, IProceedToReconnectState proceedToReconnectState, IProceedToCreateProfileState proceedToCreateProfileState, IProceedToTutorialState proceedToTutorialState, IGetBackendSession getBackendSession, IExecuteLogout executeLogout, IShouldCreateProfile shouldCreateProfile, IShouldPlayTutorial shouldPlayTutorial, IShouldReconnectToRunningMatch shouldReconnectToRunningMatch, WelcomeStatePendingLoadingStorage welcomeStatePendingLoadingStorage, ILogger<ExecuteWelcomeState> logger)
		{
			this._welcomePresenter = welcomePresenter;
			this._proceedToMainMenu = proceedToMainMenu;
			this._proceedToReconnectState = proceedToReconnectState;
			this._proceedToCreateProfileState = proceedToCreateProfileState;
			this._proceedToTutorialState = proceedToTutorialState;
			this._getBackendSession = getBackendSession;
			this._executeLogout = executeLogout;
			this._shouldCreateProfile = shouldCreateProfile;
			this._shouldPlayTutorial = shouldPlayTutorial;
			this._shouldReconnectToRunningMatch = shouldReconnectToRunningMatch;
			this._welcomeStatePendingLoadingStorage = welcomeStatePendingLoadingStorage;
			this._logger = logger;
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.ContinueWith<Unit, Unit>(this.ExecutePendingLoading(), this.LogoutFromCurrentSession());
		}

		public IObservable<Unit> Execute()
		{
			return Observable.DoOnCompleted<Unit>(this.ExecuteWelcomeWindow(), new Action(this.ProceedToNextState));
		}

		private IObservable<Unit> ExecutePendingLoading()
		{
			if (this._welcomeStatePendingLoadingStorage.PendingLoading.IsNone)
			{
				return Observable.ReturnUnit();
			}
			IObservable<Unit> value = this._welcomeStatePendingLoadingStorage.PendingLoading.Value;
			this._welcomeStatePendingLoadingStorage.PendingLoading = Maybe<IObservable<Unit>>.None;
			return value;
		}

		private IObservable<Unit> LogoutFromCurrentSession()
		{
			if (this._getBackendSession.Get().IsNone)
			{
				return Observable.ReturnUnit();
			}
			return Observable.DoOnSubscribe<Unit>(this._executeLogout.Logout(), delegate()
			{
				this._logger.Info("Entering welcome state as logged in. Executing logout.");
			});
		}

		private IObservable<Unit> ExecuteWelcomeWindow()
		{
			return this._welcomePresenter.Execute();
		}

		private void ProceedToNextState()
		{
			if (this._shouldPlayTutorial.Check())
			{
				this.ProceedToTutorialState();
				return;
			}
			if (this._shouldCreateProfile.Get())
			{
				this.ProceedToProfileState();
				return;
			}
			if (this._shouldReconnectToRunningMatch.Check())
			{
				this.ProceedToReconnectState();
				return;
			}
			this.ProceedToMainMenu();
		}

		private void ProceedToTutorialState()
		{
			this._logger.Info("Player should play the tutorial. Proceeding to Tutorial State.");
			this._proceedToTutorialState.Proceed();
		}

		private void ProceedToProfileState()
		{
			this._logger.Info("Player should create profile. Proceeding to Create Profile State.");
			this._proceedToCreateProfileState.Proceed();
		}

		private void ProceedToReconnectState()
		{
			this._logger.Info("Player should reconnect to running match. Proceeding to Reconnect State.");
			this._proceedToReconnectState.Proceed();
		}

		private void ProceedToMainMenu()
		{
			this._logger.Info("Proceeding to Main Menu State.");
			this._proceedToMainMenu.Proceed();
		}

		private readonly IWelcomePresenter _welcomePresenter;

		private readonly IProceedToMainMenu _proceedToMainMenu;

		private readonly IProceedToReconnectState _proceedToReconnectState;

		private readonly IProceedToCreateProfileState _proceedToCreateProfileState;

		private readonly IProceedToTutorialState _proceedToTutorialState;

		private readonly IExecuteLogout _executeLogout;

		private readonly IShouldCreateProfile _shouldCreateProfile;

		private readonly IShouldPlayTutorial _shouldPlayTutorial;

		private readonly IShouldReconnectToRunningMatch _shouldReconnectToRunningMatch;

		private readonly WelcomeStatePendingLoadingStorage _welcomeStatePendingLoadingStorage;

		private readonly ILogger<ExecuteWelcomeState> _logger;

		private readonly IGetBackendSession _getBackendSession;
	}
}
