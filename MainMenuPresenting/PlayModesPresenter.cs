using System;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.CompetitiveMode.View.Matchmaking;
using HeavyMetalMachines.Crossplay;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Localization.Business;
using HeavyMetalMachines.Matchmaking.Queue;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Extensions;
using HeavyMetalMachines.Presenting.Navigation;
using HeavyMetalMachines.Training.Business;
using HeavyMetalMachines.Training.Presenter;
using Hoplon.Logging;
using Standard_Assets.Scripts.HMM.Tutorial;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public class PlayModesPresenter : IPlayModesPresenter, IPresenter
	{
		public PlayModesPresenter(IViewProvider viewProvider, ICompetitiveQueueJoinPresenter competitiveQueueJoinPresenter, ITrainingModesBusinessFactory trainingBusinessFactory, IMainMenuPresenterTree mainMenuPresenterTree, ILogger<PlayModesPresenter> logger, ISearchCasualMatch searchNormalMatch, ITrainingPopUpPresenter trainingPopUpPresenter, ITrainingPopUpRules trainingPopUpRules, ICheckPlayerHasDoneTutorial checkPlayerHasDoneTutorial, IObserveCrossplayChange observeCrossplayChange, IGetLocalizedCrossplayActivation getLocalizedCrossplayActivation)
		{
			this._viewProvider = viewProvider;
			this._competitiveQueueJoinPresenter = competitiveQueueJoinPresenter;
			this._mainMenuPresenterTree = mainMenuPresenterTree;
			this._trainingBusinessFactory = trainingBusinessFactory;
			this._logger = logger;
			this._searchNormalMatch = searchNormalMatch;
			this._trainingPopUpPresenter = trainingPopUpPresenter;
			this._trainingPopUpRules = trainingPopUpRules;
			this._checkPlayerHasDoneTutorial = checkPlayerHasDoneTutorial;
			this._observeCrossplayChange = observeCrossplayChange;
			this._getLocalizedCrossplayActivation = getLocalizedCrossplayActivation;
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this.InitializeView();
			}), (Unit _) => this.InitializeJoinPresenter());
		}

		private void InitializeView()
		{
			this._view = this._viewProvider.Provide<IPlayModesView>(null);
			this._view.BackButton.IsInteractable = false;
			this._view.OpenTrainingModeButton.IsInteractable = false;
			this._view.OpenCustomModeButton.IsInteractable = false;
			this._view.OpenNormalModeButton.IsInteractable = false;
			this._view.OpenCompetitiveModeInfoButton.IsInteractable = false;
			this._disposables = new CompositeDisposable();
			this.ObserveOwnNode();
			this.ResetModesArtsAlpha();
			this.InitializeCrossplayObserve();
		}

		private void InitializeButtons()
		{
			this.InitializeTrainingModeButton();
			this.EnableCompetitiveModeInfoButtonInteractivity();
			this.EnableCustomModeButtonInteractivity();
			this.EnableBackButtonInteractivity();
			this.EnableNormalModeButtonInteractivity();
			this.EnableRegionButtonInteractivity();
		}

		private void InitializeCrossplayObserve()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<bool>(Observable.Do<bool>(this._observeCrossplayChange.GetAndObserve(), delegate(bool isCrossplayEnabled)
			{
				this.UpdateCrossplayIndicatorLabel(isCrossplayEnabled);
			}));
			this._disposables.Add(disposable);
		}

		private void UpdateCrossplayIndicatorLabel(bool isCrossplayEnabled)
		{
			string text;
			if (isCrossplayEnabled)
			{
				text = this._getLocalizedCrossplayActivation.GetCrossplayEnabledGenericMessage();
			}
			else
			{
				text = this._getLocalizedCrossplayActivation.GetCrossplayDisabledGenericMessage();
			}
			this._view.CrossplayActivatedLabel.Text = text;
		}

		private void ObserveOwnNode()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<IPresenterNode>(Observable.Do<IPresenterNode>(Observable.Do<IPresenterNode>(this._mainMenuPresenterTree.PresenterTree.ObserveNodeEnter(this._mainMenuPresenterTree.PlayModesNode), delegate(IPresenterNode _)
			{
				this._competitiveQueueJoinPresenter.Enable();
			}), delegate(IPresenterNode _)
			{
				this.ResetModesArtsAlpha();
			}));
			this._disposables.Add(disposable);
			IDisposable disposable2 = ObservableExtensions.Subscribe<IPresenterNode>(Observable.Do<IPresenterNode>(Observable.Do<IPresenterNode>(this._mainMenuPresenterTree.PresenterTree.ObserveNodeLeave(this._mainMenuPresenterTree.PlayModesNode), delegate(IPresenterNode _)
			{
				this._competitiveQueueJoinPresenter.Disable();
			}), delegate(IPresenterNode _)
			{
				this.ResetModesArtsAlpha();
			}));
			this._disposables.Add(disposable2);
			this.InitializeExclusiveButtons();
		}

		private void InitializeExclusiveButtons()
		{
			IObservable<Unit> observable = PresentingObservable.ExclusivelyExecuteButtons().AddButtonClickAction(this._view.BackButton, this.BackButtonClickLogic()).AddButtonClickAction(this._view.OpenNormalModeButton, this.NormalButtonClickLogic()).Build();
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(observable);
			this._disposables.Add(disposable);
		}

		private void OnStartTutorialButtonClick()
		{
			this._buttonBILogger.LogButtonClick(ButtonName.Tutorial);
			ObservableExtensions.Subscribe<bool>(Observable.Do<bool>(Observable.Where<bool>(this._dialogPresenter.ShowQuestionWindow(this.GetStartTutorialQuestionConfig()), (bool accept) => accept), delegate(bool _)
			{
				this.ConfirmTutorialButtonClick();
			}));
		}

		private QuestionConfiguration GetStartTutorialQuestionConfig()
		{
			string key = this._checkPlayerHasDoneTutorial.CheckForLocalPlayer() ? "AskPlayAgainTuto" : "AskFirstPlayOnStartGame";
			return new QuestionConfiguration
			{
				AcceptMessage = Language.Get("Yes", TranslationContext.GUI),
				DeclineMessage = Language.Get("No", TranslationContext.GUI),
				Message = Language.Get(key, TranslationContext.Tutorial)
			};
		}

		private void ConfirmTutorialButtonClick()
		{
			this._waitingWindow.Show(typeof(PlayModesPresenter));
			string config = this.GetTutorialArenaConfig();
			IMatchmakingTrainingQueueJoin matchmakingTrainingQueueJoin = this._trainingBusinessFactory.CreateJoinCustomTraining();
			ObservableExtensions.Subscribe<Unit>(matchmakingTrainingQueueJoin.JoinTraining(config), delegate(Unit _)
			{
				this._logger.InfoFormat("Joining Training. Config ={0}", new object[]
				{
					config
				});
			}, delegate(Exception err)
			{
				this._logger.InfoFormat("Error while Joining Training. Config ={0} err={1}", new object[]
				{
					config,
					err
				});
			}, delegate()
			{
				this._waitingWindow.Hide(typeof(PlayModesPresenter));
			});
		}

		private IObservable<Unit> BackButtonClickLogic()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._buttonBILogger.LogButtonClick(ButtonName.GameModeBack);
				this._view.UiNavigationAxisSelector.ClearSelection();
				return this._mainMenuPresenterTree.PresenterTree.NavigateBackwards();
			});
		}

		private IObservable<Unit> NormalButtonClickLogic()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._buttonBILogger.LogButtonClick(ButtonName.GameModeNormal);
				this._searchNormalMatch.Search();
				return Observable.ReturnUnit();
			});
		}

		private void EnableCustomModeButtonInteractivity()
		{
			this._view.OpenCustomModeButton.IsInteractable = true;
		}

		private void EnableBackButtonInteractivity()
		{
			this._view.BackButton.IsInteractable = true;
		}

		private void EnableNormalModeButtonInteractivity()
		{
			this._view.OpenNormalModeButton.IsInteractable = true;
		}

		private void EnableRegionButtonInteractivity()
		{
			ActivatableExtensions.Activate(this._view.RegionButton);
		}

		private string GetTutorialArenaConfig()
		{
			return "MatchKind:Tutorial:ArenaIndex:0";
		}

		private void InitializeTrainingModeButton()
		{
			this._view.OpenTrainingModeButton.SetActive(true);
			this._view.OpenTrainingModeButton.IsInteractable = true;
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(this._view.OpenTrainingModeButton.OnClick(), delegate(Unit _)
			{
				this._buttonBILogger.LogButtonClick(ButtonName.GameModeTraining);
			});
			this._disposables.Add(disposable);
			IDisposable disposable2 = ButtonExtensions.AddNavigationToNode(this._view.OpenTrainingModeButton, this._mainMenuPresenterTree.PresenterTree, this._mainMenuPresenterTree.TrainingSelectionNode);
			this._disposables.Add(disposable2);
		}

		private void EnableCompetitiveModeInfoButtonInteractivity()
		{
			ActivatableExtensions.Activate(this._view.OpenCompetitiveModeInfoActivatable);
			this._view.OpenCompetitiveModeInfoButton.IsInteractable = true;
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(ButtonExtensions.OnClickNavigateToNode(this._view.OpenCompetitiveModeInfoButton, this._mainMenuPresenterTree.PresenterTree, this._mainMenuPresenterTree.CompetitiveModeNode));
			this._disposables.Add(disposable);
		}

		private void ResetModesArtsAlpha()
		{
			foreach (IAlpha alpha in this._view.ModesArtsAlphas)
			{
				alpha.Alpha = 0f;
			}
		}

		private IObservable<Unit> InitializeJoinPresenter()
		{
			this._disposables.Add(this._competitiveQueueJoinPresenter);
			this._competitiveQueueJoinPresenter.ViewProviderContext = NguiCompetitiveQueueJoinView.ViewProviderContext;
			return Observable.Do<Unit>(Observable.DelayFrame<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				ActivatableExtensions.Activate(this._view.Group);
			}), delegate(Unit _)
			{
				ActivatableExtensions.Activate(this._view.CompetitiveGroup);
			}), 1, 0), delegate(Unit _)
			{
				this._competitiveQueueJoinPresenter.Initialize();
			});
		}

		public IObservable<Unit> Show()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._view.RootObject.SetActive(true);
				this._view.RootPanel.Alpha = 1f;
				this.InitializeButtons();
				this._view.UiNavigationGroupHolder.AddGroup();
				return Observable.Merge<Unit>(new IObservable<Unit>[]
				{
					this._view.BackGroundAnimatinIn.Play(),
					this._view.ModesAnimationIn.Play()
				});
			});
		}

		public IObservable<Unit> Hide()
		{
			return Observable.DoOnCompleted<Unit>(Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				this._view.BackGroundAnimatinOut.Play(),
				this._view.ModesAnimationOut.Play()
			}), delegate()
			{
				this._view.UiNavigationGroupHolder.RemoveGroup();
				this.OnHideCompleted();
			});
		}

		public void OnHideCompleted()
		{
			this._view.RootObject.SetActive(false);
			this._view.RootPanel.Alpha = 0f;
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._disposables.Dispose();
			});
		}

		public IObservable<Unit> ObserveHide()
		{
			throw new NotImplementedException();
		}

		private readonly ILogger<PlayModesPresenter> _logger;

		private readonly IViewProvider _viewProvider;

		private readonly ICompetitiveQueueJoinPresenter _competitiveQueueJoinPresenter;

		private readonly IMainMenuPresenterTree _mainMenuPresenterTree;

		private readonly ISearchCasualMatch _searchNormalMatch;

		private readonly ITrainingPopUpPresenter _trainingPopUpPresenter;

		private readonly ITrainingPopUpRules _trainingPopUpRules;

		private readonly ICheckPlayerHasDoneTutorial _checkPlayerHasDoneTutorial;

		private readonly IObserveCrossplayChange _observeCrossplayChange;

		private readonly IGetLocalizedCrossplayActivation _getLocalizedCrossplayActivation;

		private CompositeDisposable _disposables;

		private IPlayModesView _view;

		private ITrainingModesBusinessFactory _trainingBusinessFactory;

		[Inject]
		private IWaitingWindow _waitingWindow;

		[Inject]
		private IDialogPresenter _dialogPresenter;

		[Inject]
		private IClientButtonBILogger _buttonBILogger;
	}
}
