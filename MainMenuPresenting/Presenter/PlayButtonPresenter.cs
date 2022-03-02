using System;
using System.Linq;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.MainMenuPresenting.View;
using HeavyMetalMachines.MatchMaking;
using HeavyMetalMachines.PlayModes.Business;
using HeavyMetalMachines.Presenting;
using Hoplon.Input;
using Hoplon.Localization.TranslationTable;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.MainMenuPresenting.Presenter
{
	public class PlayButtonPresenter : IStartMenuPlayButtonPresenter, IPresenter
	{
		public PlayButtonPresenter(IViewProvider viewProvider, IGetThenObservePlayModesNavegability getThenObservePlayModesNavegability, ILocalizeKey translation, ICancelMatchmakingMatchSearch cancelMatchmakingSearch, IDialogPresenter dialogPresenter, IInputGetActiveDevicePoller getActiveDevicePoller, IClientButtonBILogger clientButtonBiLogger, ILogger<PlayButtonPresenter> logger)
		{
			this._viewProvider = viewProvider;
			this._getThenObservePlayModesNavegability = getThenObservePlayModesNavegability;
			this._translation = translation;
			this._cancelMatchmakingSearch = cancelMatchmakingSearch;
			this._dialogPresenter = dialogPresenter;
			this._getActiveDevicePoller = getActiveDevicePoller;
			this._clientButtonBiLogger = clientButtonBiLogger;
			this._logger = logger;
		}

		public IObservable<Unit> Initialize()
		{
			this._view = this._viewProvider.Provide<IStartMenuPlayModesView>(null);
			this._view.PlayLabel.Text = this._translation.Get("PLAY_BUTTON", TranslationContext.MainMenuGui);
			this._disposables = new CompositeDisposable();
			return Observable.ReturnUnit();
		}

		private void InitializeNavigabilityObservation()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(Observable.SelectMany<PlayModesNavegabilityResult, Unit>(this._getThenObservePlayModesNavegability.GetThenObserve(), (PlayModesNavegabilityResult result) => this.UpdateView(result)));
			this._disposables.Add(disposable);
		}

		private IObservable<Unit> UpdateView(PlayModesNavegabilityResult state)
		{
			if (state.CanNavigate)
			{
				this.UpdateNavigableView();
				this._logger.InfoFormat("Navigable Play Modes", new object[0]);
			}
			else
			{
				this.UpdateUnnavigableView(state.Reasons.First<PlayModesNavegabilityReason>());
				for (int i = 0; i < state.Reasons.Length; i++)
				{
					this._logger.InfoFormat("Unnavigable Play Modes. Reason:{0}", new object[]
					{
						state.Reasons[i]
					});
				}
			}
			return Observable.ReturnUnit();
		}

		private void UpdateNavigableView()
		{
			this._view.PlayActivatable.SetActive(true);
			this._view.WaitingActivatable.SetActive(false);
			this._view.TimerActivatable.SetActive(false);
			this._view.CancelSearchButton.SetActive(false);
			this.DisposeCancelSearch();
			this._view.AxisSelectorTransformHandler.TryForceSelection(this._view.AxisSelectorTransformHandler.SelectedTransform);
		}

		private void UpdateUnnavigableView(PlayModesNavegabilityReason reason)
		{
			this._view.PlayActivatable.SetActive(false);
			this._view.WaitingActivatable.SetActive(true);
			this._view.TimerActivatable.SetActive(true);
			this._view.CancelSearchButton.SetActive(false);
			this.DisposeCancelSearch();
			switch (reason)
			{
			case PlayModesNavegabilityReason.PlayerIsAlreadyInQueue:
				this.EnableCancelSearchButton();
				break;
			case PlayModesNavegabilityReason.PlayerIsNotLeaderOfGroup:
				this._view.WaitingLabel.Text = this._translation.Get("WAITINGGROUPMEMBER", TranslationContext.MainMenuGui);
				break;
			case PlayModesNavegabilityReason.GroupIsNotReady:
				this._view.WaitingLabel.Text = this._translation.Get("WAITINGGROUPMEMBER", TranslationContext.MainMenuGui);
				break;
			default:
				throw new NotImplementedException("Unknown PlayModesNavegabilityReason: " + reason);
			}
		}

		private void EnableCancelSearchButton()
		{
			this._view.WaitingActivatable.SetActive(false);
			this._view.TimerActivatable.SetActive(false);
			this._view.CancelSearchButton.SetActive(true);
			this._cancelSearchDisposable = ObservableExtensions.Subscribe<bool>(Observable.Repeat<bool>(Observable.Do<bool>(Observable.Do<bool>(Observable.Do<bool>(Observable.Where<bool>(Observable.SelectMany<Unit, bool>(Observable.Do<Unit>(Observable.First<Unit>(this._view.CancelSearchButton.OnClick()), delegate(Unit _)
			{
				this._clientButtonBiLogger.LogButtonClick(ButtonName.MatchmakingCancelOnMainMenu);
			}), this._dialogPresenter.ShowQuestionWindow(this.GetCancelSearchQuestionConfig())), (bool accept) => accept), delegate(bool _)
			{
				this._clientButtonBiLogger.LogButtonClick(ButtonName.MatchmakingCancelOnMainMenuAccept);
			}), delegate(bool _)
			{
				this._cancelMatchmakingSearch.Cancel();
			}), delegate(bool _)
			{
				this.TrySelectPlayButton();
			})));
		}

		private QuestionConfiguration GetCancelSearchQuestionConfig()
		{
			return new QuestionConfiguration
			{
				AcceptMessage = Language.Get("Yes", TranslationContext.GUI),
				DeclineMessage = Language.Get("No", TranslationContext.GUI),
				Message = this._translation.Get("MAINMENU_EXIT_MATCH_QUESTION", TranslationContext.MainMenuGui)
			};
		}

		private void TrySelectPlayButton()
		{
			if (this._getActiveDevicePoller.GetActiveDevice() == 3)
			{
				this._view.UiNavigationSelectionOnPlayButton();
			}
		}

		public IObservable<Unit> Show()
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this.InitializeNavigabilityObservation();
			});
		}

		public IObservable<Unit> Hide()
		{
			return Observable.ReturnUnit();
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.Do<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._disposables.Dispose();
			}), delegate(Unit _)
			{
				this.DisposeCancelSearch();
			});
		}

		private void DisposeCancelSearch()
		{
			if (this._cancelSearchDisposable != null)
			{
				this._cancelSearchDisposable.Dispose();
				this._cancelSearchDisposable = null;
			}
		}

		public IObservable<Unit> ObserveHide()
		{
			throw new NotImplementedException();
		}

		private readonly IViewProvider _viewProvider;

		private readonly IGetThenObservePlayModesNavegability _getThenObservePlayModesNavegability;

		private readonly ILocalizeKey _translation;

		private readonly ICancelMatchmakingMatchSearch _cancelMatchmakingSearch;

		private readonly IDialogPresenter _dialogPresenter;

		private readonly IInputGetActiveDevicePoller _getActiveDevicePoller;

		private readonly IClientButtonBILogger _clientButtonBiLogger;

		private readonly ILogger<PlayButtonPresenter> _logger;

		private IStartMenuPlayModesView _view;

		private CompositeDisposable _disposables;

		private IDisposable _cancelSearchDisposable;
	}
}
