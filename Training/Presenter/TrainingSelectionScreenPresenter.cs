using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.Arena.Infra;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Frontend.Region;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.Matches.DataTransferObjects;
using HeavyMetalMachines.Matchmaking.Queue;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Extensions;
using HeavyMetalMachines.Training.Business;
using HeavyMetalMachines.Training.View;
using Hoplon.Localization.TranslationTable;
using Hoplon.Logging;
using Hoplon.Reactive;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Training.Presenter
{
	public class TrainingSelectionScreenPresenter : ITrainingSelectionScreenPresenter, IPresenter
	{
		public TrainingSelectionScreenPresenter(ILogger<TrainingSelectionScreenPresenter> logger)
		{
			this._logger = logger;
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.AsUnitObservable<Unit>(Observable.Do<Unit>(this._viewLoader.LoadView("UI_ADD_TrainingMode"), delegate(Unit _)
			{
				this.InitializeView();
			}));
		}

		public IObservable<Unit> Show()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._view.Canvas.Enable();
				this._view.BackButton.IsInteractable = false;
				return Observable.Do<Unit>(this._view.ShowAnimation.Play(), delegate(Unit _)
				{
					this.EnableButtons();
					this._view.UiNavigationGroupHolder.AddGroup();
				});
			});
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Defer<Unit>(delegate()
			{
				for (int i = 0; i < this._view.SelectionViews.Length; i++)
				{
					this._view.SelectionViews[i].MainButton.IsInteractable = false;
				}
				return Observable.Do<Unit>(this._view.HideAnimation.Play(), delegate(Unit _)
				{
					this._view.UiNavigationGroupHolder.RemoveGroup();
					this._hideSubject.OnNext(Unit.Default);
				});
			});
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.Defer<Unit>(() => Observable.Do<Unit>(Observable.Do<Unit>(this._viewLoader.UnloadView("UI_ADD_TrainingMode"), delegate(Unit _)
			{
				this._regionStatusPresenter.Dispose();
			}), delegate(Unit _)
			{
				this._disposables.Dispose();
			}));
		}

		public IObservable<Unit> ObserveHide()
		{
			return this._hideSubject;
		}

		private void InitializeView()
		{
			this._disposables = new CompositeDisposable();
			this._view = this._viewProvider.Provide<ITrainingSelectionScreenView>(null);
			this._regionStatusPresenter.Initialize();
			this.InitializeTitle();
			this.InitializeBackButton();
			this.InitializeArenaButtons();
		}

		private void InitializeTitle()
		{
			this._view.Title.Title = this._translation.Get("TRAINING_TITLE", TranslationContext.TrainingMode);
			this._view.Title.InfoButton.SetActive(false);
			this._view.Title.SubtitleActivatable.SetActive(false);
			this._view.Title.DescriptionActivatable.SetActive(false);
		}

		private void EnableButtons()
		{
			this._view.BackButton.IsInteractable = true;
			for (int i = 0; i < this._view.SelectionViews.Length; i++)
			{
				this._view.SelectionViews[i].MainButton.IsInteractable = true;
			}
		}

		private void InitializeBackButton()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(this._view.BackButton.OnClick(), delegate(Unit _)
			{
				this._buttonBILogger.LogButtonClick(ButtonName.TrainingBack);
			});
			IDisposable disposable2 = ButtonExtensions.AddNavigationBackwards(this._view.BackButton, this._mainMenuPresenterTree.PresenterTree);
			this._disposables.Add(disposable);
			this._disposables.Add(disposable2);
		}

		private void InitializeArenaButtons()
		{
			for (int i = 0; i < this._view.SelectionViews.Length; i++)
			{
				ITrainingSelectionView selectionView = this._view.SelectionViews[i];
				selectionView.MainButton.IsInteractable = false;
				int arenaIndex = selectionView.ArenaIndex;
				IDisposable disposable = ObservableExtensions.Subscribe<Unit>(ObservableExtensions.IfElse<Unit, Unit>(selectionView.MainButton.OnClick(), (Unit _) => this.IsOnGroup(), this.OpenConfimartionWindow(arenaIndex), this.GoToGame(arenaIndex)), delegate(Unit _)
				{
					this._buttonBILogger.LogButtonClick(selectionView.BiButtonName);
				});
				this._disposables.Add(disposable);
			}
		}

		private Func<Unit, IObservable<Unit>> GoToGame(int arenaIndex)
		{
			return (Unit __) => Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this.OnTrainingButtonClick(arenaIndex);
			});
		}

		private Func<Unit, IObservable<Unit>> OpenConfimartionWindow(int arenaIndex)
		{
			return (Unit _) => Observable.AsUnitObservable<bool>(Observable.Do<bool>(Observable.Do<bool>(Observable.Where<bool>(this._dialogPresenter.ShowQuestionWindow(this.GetQuestionConfig()), (bool accept) => accept), delegate(bool __)
			{
				this._groupManager.LeaveGroup(false);
			}), delegate(bool __)
			{
				this.OnTrainingButtonClick(arenaIndex);
			}));
		}

		private bool IsOnGroup()
		{
			return this._groupManager.IsUserInGroupOrPendingInvite;
		}

		private void OnTrainingButtonClick(int arenaId)
		{
			this._waitingWindow.Show(typeof(TrainingSelectionScreenPresenter));
			string config = this.GetArenaConfigurationString(arenaId);
			IMatchmakingTrainingQueueJoin matchmakingTrainingQueueJoin = this._trainingBusinessFactory.CreateJoinCustomTraining();
			ObservableExtensions.Subscribe<Unit>(Observable.DoOnTerminate<Unit>(matchmakingTrainingQueueJoin.JoinTraining(config), delegate()
			{
				this._waitingWindow.Hide(typeof(TrainingSelectionScreenPresenter));
			}), delegate(Unit __)
			{
				this._logger.InfoFormat("Joining Training. Config ={0}", new object[]
				{
					config
				});
			}, delegate(Exception err)
			{
				this._logger.WarnFormat("Error while Joining Training. Config ={0} err={1}", new object[]
				{
					config,
					err
				});
			});
		}

		private string GetArenaConfigurationString(int arenaIndex)
		{
			IGameArenaInfo arenaByIndex = this._gameArenaConfigProvider.GameArenaConfig.GetArenaByIndex(arenaIndex);
			return string.Format("MatchKind:{0}:ArenaIndex:{1}", this.GetMatchKindFromArena(arenaByIndex), arenaIndex);
		}

		private MatchKind GetMatchKindFromArena(IGameArenaInfo gameArenaInfo)
		{
			return (!gameArenaInfo.IsTutorial) ? 6 : 2;
		}

		private QuestionConfiguration GetQuestionConfig()
		{
			return new QuestionConfiguration
			{
				AcceptMessage = Language.Get("Yes", TranslationContext.GUI),
				DeclineMessage = Language.Get("No", TranslationContext.GUI),
				Message = Language.Get("TRAINING_MODE_LEAVE_GROUP", TranslationContext.TrainingMode)
			};
		}

		private const string SceneName = "UI_ADD_TrainingMode";

		[Inject]
		private ILocalizeKey _translation;

		[Inject]
		private IViewLoader _viewLoader;

		[Inject]
		private IViewProvider _viewProvider;

		[Inject]
		private IMainMenuPresenterTree _mainMenuPresenterTree;

		[Inject]
		private IWaitingWindow _waitingWindow;

		[Inject]
		private ITrainingModesBusinessFactory _trainingBusinessFactory;

		[Inject]
		private IGameArenaConfigProvider _gameArenaConfigProvider;

		[Inject]
		private IDialogPresenter _dialogPresenter;

		[Inject]
		private GroupManager _groupManager;

		[Inject]
		private IClientButtonBILogger _buttonBILogger;

		[Inject]
		private IRegionStatusPresenter _regionStatusPresenter;

		private ITrainingSelectionScreenView _view;

		private readonly Subject<Unit> _hideSubject = new Subject<Unit>();

		private CompositeDisposable _disposables;

		private readonly ILogger<TrainingSelectionScreenPresenter> _logger;
	}
}
