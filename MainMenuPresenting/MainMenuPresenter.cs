using System;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.MainMenuPresenting.Presenter;
using HeavyMetalMachines.MainMenuView.Presenter;
using HeavyMetalMachines.News.Presenting;
using HeavyMetalMachines.OpenUrl;
using HeavyMetalMachines.Options.Presenting;
using HeavyMetalMachines.ParentalControl;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.PlayModes.Business;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Extensions;
using HeavyMetalMachines.Presenting.Navigation;
using HeavyMetalMachines.Social.Buttons;
using HeavyMetalMachines.Swordfish.Session;
using HeavyMetalMachines.Training.Business;
using HeavyMetalMachines.Training.Presenter;
using Hoplon.Input;
using Hoplon.Input.UiNavigation.AxisSelector;
using Pocketverse;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public class MainMenuPresenter : IMainMenuPresenter, IPresenter
	{
		public MainMenuPresenter(IViewProvider viewProvider, IClientButtonBILogger buttonBI, IClientShopBILogger clientShopBiLogger, IMainMenuPresenterTree mainMenuPresenterTree, IStartMenuPlayButtonPresenter playModesPresenter, IGetThenObservePlayModesNavegability getThenObservePlayModesNavegability, IOptionsPresenter optionsPresenter, IInputGetActiveDevicePoller inputGetActiveDevicePoller, INewsPresenter newsPresenter, ISocialAndNewsInteractionController socialAndNewsInteractionController, IConfigLoader configLoader, IGetLocalPlayer getLocalPlayer, ILoginSessionIdProvider loginSessionIdProvider, IOpenUrl openUrl, ISocialButtonsOpenUrl socialButtonsOpenUrl, IOpenUrlUgcRestricted openUrlUgcRestricted, IGetUGCRestrictionIsEnabled getUgcRestrictionIsEnabled, IUGCRestrictionDialogPresenter ugcRestrictionDialogPresenter, ITrainingPopUpRules trainingPopUpRules, ITrainingPopUpPresenter trainingPopUpPresenter)
		{
			this._viewProvider = viewProvider;
			this._buttonBILogger = buttonBI;
			this._clientShopBiLogger = clientShopBiLogger;
			this._mainMenuPresenterTree = mainMenuPresenterTree;
			this._playButtonPresenter = playModesPresenter;
			this._getThenObservePlayModesNavegability = getThenObservePlayModesNavegability;
			this._optionsPresenter = optionsPresenter;
			this._newsPresenter = newsPresenter;
			this._inputGetActiveDevicePoller = inputGetActiveDevicePoller;
			this._socialAndNewsInteractionController = socialAndNewsInteractionController;
			this._configLoader = configLoader;
			this._getLocalPlayer = getLocalPlayer;
			this._loginSessionIdProvider = loginSessionIdProvider;
			this._openUrl = openUrl;
			this._socialButtonsOpenUrl = socialButtonsOpenUrl;
			this._openUrlUgcRestricted = openUrlUgcRestricted;
			this._getUgcRestrictionIsEnabled = getUgcRestrictionIsEnabled;
			this._ugcRestrictionDialogPresenter = ugcRestrictionDialogPresenter;
			this._trainingPopUpRules = trainingPopUpRules;
			this._trainingPopUpPresenter = trainingPopUpPresenter;
		}

		public IObservable<Unit> Initialize()
		{
			this._view = this._viewProvider.Provide<IMainMenuView>(null);
			this._disposables = new CompositeDisposable();
			this.ChangeExternalLinkButtonsInteractability(false);
			this._socialAndNewsInteractionController.Initialize();
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(this._view.UiNavigationGroupHolder.ObserveInputCancelDown(), delegate(Unit _)
			{
				this.OnInputCancelDown();
			});
			this._disposables.Add(disposable);
			this.InitializeEdgeDetectionToFocusNews();
			return this._playButtonPresenter.Initialize();
		}

		private void InitializeEdgeDetectionToFocusNews()
		{
			this._disposables.Add(ObservableExtensions.Subscribe<AxisSelectorEdge>(Observable.Do<AxisSelectorEdge>(Observable.Where<AxisSelectorEdge>(this._view.UiNavigationEdgeNotifier.ObserveOnEdgeReached(), (AxisSelectorEdge axisSelectorEdge) => axisSelectorEdge == 3), delegate(AxisSelectorEdge _)
			{
				this.FocusNews();
			})));
		}

		private void FocusNews()
		{
			this._newsPresenter.GetUiNavigationFocus();
			this._view.UiNavigationAxisSelector.ClearSelection();
		}

		private void ChangeExternalLinkButtonsInteractability(bool isInteractable)
		{
			this._view.PlayButton.IsInteractable = isInteractable;
			this._view.StoreButton.IsInteractable = isInteractable;
			this._view.BattlepassButton.IsInteractable = isInteractable;
			this._view.TournamentButton.IsInteractable = isInteractable;
			this._view.InventoryButton.IsInteractable = isInteractable;
			this._view.ProfileButton.IsInteractable = isInteractable;
			this._view.SpectatorButton.IsInteractable = isInteractable;
			this._view.RankingButton.IsInteractable = isInteractable;
			this._view.TrainingButton.IsInteractable = isInteractable;
			this._view.TeamsButton.IsInteractable = isInteractable;
			this._view.HelpButton.IsInteractable = isInteractable;
			this._view.DiscordButton.IsInteractable = isInteractable;
			this._view.MetalSponsorsButton.IsInteractable = isInteractable;
		}

		private void InitializeButtons()
		{
			this.InitializePlayButton();
			this.InitializeNavigationAndActivateButton(this._view.StoreButton, this._mainMenuPresenterTree.StoreDriversNode, ButtonName.Shop);
			this.InitializeNavigationAndActivateButton(this._view.BattlepassButton, this._mainMenuPresenterTree.BattlepassNode, ButtonName.Metalpass);
			this.InitializeNavigationAndActivateButton(this._view.RankingButton, this._mainMenuPresenterTree.MainMenuCompetitiveModeNode, ButtonName.Ranked);
			this.InitializeNavigationAndActivateButton(this._view.InventoryButton, this._mainMenuPresenterTree.InventorySpraysNode, ButtonName.Inventory);
			this.InitializeNavigationAndActivateButton(this._view.ProfileButton, this._mainMenuPresenterTree.ProfileNode, ButtonName.Profile);
			this.InitializeNavigationAndActivateButton(this._view.SpectatorButton, this._mainMenuPresenterTree.SpectatorNode, ButtonName.Storyteller);
			this.InitializeNavigationAndActivateButton(this._view.TournamentButton, this._mainMenuPresenterTree.TournamentListNode, ButtonName.TournamentList);
			this.InitializeTrainingShortcut();
			this.InitializeTeamsButton();
			this.InitializeMetalSponsorsButton();
			this.InitializeHelpButton();
			this.InitializeDiscordButton();
			this.InitializeStoreButtonClickObservationForBiLog();
		}

		private void InitializeStoreButtonClickObservationForBiLog()
		{
			this._disposables.Add(ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._view.StoreButton.OnClick(), delegate(Unit _)
			{
				this._clientShopBiLogger.Log(0, 0);
			})));
		}

		private void InitializeNavigation(IButton button, IPresenterNode node, ButtonNameInstance biName)
		{
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(button.OnClick(), delegate(Unit _)
			{
				this._buttonBILogger.LogButtonClick(biName);
				button.IsInteractable = false;
				ObservableExtensions.Subscribe<Unit>(this._mainMenuPresenterTree.PresenterTree.NavigateToNode(node), delegate(Unit onNext)
				{
					button.IsInteractable = true;
				}, delegate(Exception onError)
				{
					throw onError;
				});
			});
			this._disposables.Add(disposable);
		}

		private void InitializePlayButton()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(Observable.SelectMany<Unit, Unit>(Observable.Do<Unit>(this._view.PlayButton.OnClick(), delegate(Unit _)
			{
				this._buttonBILogger.LogButtonClick(ButtonName.Play);
				this._view.PlayButton.IsInteractable = false;
			}), (Unit _) => this.CheckForEntryInNormalModeQueue()));
			this._disposables.Add(disposable);
		}

		private IObservable<Unit> CheckForEntryInNormalModeQueue()
		{
			return Observable.Defer<Unit>(delegate()
			{
				if (this._trainingPopUpRules.CanOpenPopUp())
				{
					return Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(this._trainingPopUpPresenter.Initialize(), (Unit _) => this._trainingPopUpPresenter.ShowAndWaitForConclusion()), delegate(Unit _)
					{
						this._view.PlayButton.IsInteractable = true;
					});
				}
				ObservableExtensions.Subscribe<Unit>(this._mainMenuPresenterTree.PresenterTree.NavigateToNode(this._mainMenuPresenterTree.PlayModesNode), delegate(Unit onNext)
				{
					this._view.PlayButton.IsInteractable = true;
				}, delegate(Exception onError)
				{
					throw onError;
				});
				return Observable.ReturnUnit();
			});
		}

		private void InitializeNavigationAndActivateButton(IButton button, IPresenterNode node, ButtonNameInstance biName)
		{
			IDisposable disposable = ButtonExtensions.InitializeNavigationAndBiToNode(button, this._mainMenuPresenterTree.PresenterTree, node, this._buttonBILogger, biName);
			this._disposables.Add(disposable);
		}

		private void InitializeTrainingShortcut()
		{
			this.InitializeNavigation(this._view.TrainingButton, this._mainMenuPresenterTree.MainMenuTrainingSelectionNode, ButtonName.Training);
			this.InitializeTrainingNavigabilityObservation();
		}

		private void InitializeTrainingNavigabilityObservation()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(Observable.SelectMany<PlayModesNavegabilityResult, Unit>(this._getThenObservePlayModesNavegability.GetThenObserve(), (PlayModesNavegabilityResult result) => this.UpdateTrainingButtonView(result)));
			this._disposables.Add(disposable);
		}

		private IObservable<Unit> UpdateTrainingButtonView(PlayModesNavegabilityResult state)
		{
			this._view.TrainingButton.IsInteractable = state.CanNavigate;
			return Observable.ReturnUnit();
		}

		private void InitializeTeamsButton()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(this._view.TeamsButton.OnClick(), delegate(Unit _)
			{
				this._buttonBILogger.LogButtonClick(ButtonName.Team);
				ObservableExtensions.Subscribe<bool>(this._openUrlUgcRestricted.OpenUrlAfterRestrictionCheck(7));
			});
			this._disposables.Add(disposable);
		}

		private void InitializeMetalSponsorsButton()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(this._view.MetalSponsorsButton.OnClick(), delegate(Unit _)
			{
				this._buttonBILogger.LogButtonClick(ButtonName.MetalSponsors);
				ObservableExtensions.Subscribe<bool>(this._openUrlUgcRestricted.OpenUrlAfterRestrictionCheck(5));
			});
			this._disposables.Add(disposable);
		}

		private void InitializeHelpButton()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(this._view.HelpButton.OnClick(), delegate(Unit _)
			{
				this._buttonBILogger.LogButtonClick(ButtonName.Help);
				this._openUrl.Open(4);
			});
			this._disposables.Add(disposable);
		}

		private void InitializeDiscordButton()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(this._view.DiscordButton.OnClick(), delegate(Unit _)
			{
				ObservableExtensions.Subscribe<bool>(Observable.Do<bool>(this._getUgcRestrictionIsEnabled.OfferToChangeGlobalRestriction(), delegate(bool isRestricted)
				{
					if (!isRestricted)
					{
						this._socialButtonsOpenUrl.OpenDiscord();
					}
				}));
			});
			this._disposables.Add(disposable);
		}

		public IObservable<Unit> Show()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this.ChangeExternalLinkButtonsInteractability(false);
				return Observable.DoOnCompleted<Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(this._playButtonPresenter.Show(), Observable.Do<Unit>(this._view.ShowLobbyAnimation.Play(), delegate(Unit _)
				{
					Debug.Log("ShowLobbyAnimation.Play()");
				})), delegate(Unit _)
				{
					this.ShowNewsWindow();
				}), delegate()
				{
					this.ChangeExternalLinkButtonsInteractability(true);
					this.InitializeButtons();
					this._view.UiNavigationGroupHolder.AddGroup();
				});
			});
		}

		private void ShowNewsWindow()
		{
			this.TryDisposeNewsPresenter();
			this._newsPresenterShowDisposable = ObservableExtensions.Subscribe<Unit>(this._newsPresenter.Show());
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._view.UiNavigationGroupHolder.RemoveGroup();
				this.ChangeExternalLinkButtonsInteractability(false);
				this.HideNewsWindow();
				return Observable.ContinueWith<Unit, Unit>(this._playButtonPresenter.Hide(), this._view.HideLobbyAnimation.Play());
			});
		}

		private void HideNewsWindow()
		{
			this.TryDisposeNewsPresenter();
			this._newsPresenterHideDisposable = ObservableExtensions.Subscribe<Unit>(this._newsPresenter.Hide());
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.Do<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				ObservableExtensions.Subscribe<Unit>(this._playButtonPresenter.Dispose());
				this.TryDisposeNewsPresenter();
				this._disposables.Dispose();
				this._socialAndNewsInteractionController.Dispose();
			}), delegate(Unit _)
			{
				this._hideSubject.OnNext(Unit.Default);
			});
		}

		private void TryDisposeNewsPresenter()
		{
			if (this._newsPresenterShowDisposable != null)
			{
				this._newsPresenterShowDisposable.Dispose();
				this._newsPresenterShowDisposable = null;
			}
			if (this._newsPresenterHideDisposable != null)
			{
				this._newsPresenterHideDisposable.Dispose();
				this._newsPresenterHideDisposable = null;
			}
		}

		public IObservable<Unit> ObserveHide()
		{
			return this._hideSubject;
		}

		private void OnInputCancelDown()
		{
			if (this._inputGetActiveDevicePoller.GetActiveDevice() == 1)
			{
				this._optionsPresenter.Show();
			}
		}

		private readonly IViewProvider _viewProvider;

		private readonly IClientButtonBILogger _buttonBILogger;

		private readonly IClientShopBILogger _clientShopBiLogger;

		private readonly IMainMenuPresenterTree _mainMenuPresenterTree;

		private readonly IStartMenuPlayButtonPresenter _playButtonPresenter;

		private readonly IGetThenObservePlayModesNavegability _getThenObservePlayModesNavegability;

		private readonly IOptionsPresenter _optionsPresenter;

		private readonly INewsPresenter _newsPresenter;

		private readonly IInputGetActiveDevicePoller _inputGetActiveDevicePoller;

		private readonly ISocialAndNewsInteractionController _socialAndNewsInteractionController;

		private readonly IConfigLoader _configLoader;

		private readonly IGetLocalPlayer _getLocalPlayer;

		private readonly ILoginSessionIdProvider _loginSessionIdProvider;

		private readonly IOpenUrl _openUrl;

		private readonly ISocialButtonsOpenUrl _socialButtonsOpenUrl;

		private readonly IOpenUrlUgcRestricted _openUrlUgcRestricted;

		private readonly IGetUGCRestrictionIsEnabled _getUgcRestrictionIsEnabled;

		private readonly IUGCRestrictionDialogPresenter _ugcRestrictionDialogPresenter;

		private readonly ITrainingPopUpRules _trainingPopUpRules;

		private readonly ITrainingPopUpPresenter _trainingPopUpPresenter;

		private readonly Subject<Unit> _hideSubject = new Subject<Unit>();

		private IMainMenuView _view;

		private IMainMenuStorytellerView _storytellerView;

		private IDisposable _newsPresenterShowDisposable;

		private IDisposable _newsPresenterHideDisposable;

		private CompositeDisposable _disposables;
	}
}
