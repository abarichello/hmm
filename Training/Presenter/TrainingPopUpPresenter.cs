using System;
using ClientAPI;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.MatchMaking;
using HeavyMetalMachines.Matchmaking.Queue;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Training.View;
using Hoplon.Localization.TranslationTable;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.Training.Presenter
{
	public class TrainingPopUpPresenter : ITrainingPopUpPresenter
	{
		public TrainingPopUpPresenter(ILogger<TrainingPopUpPresenter> logger, IViewProvider viewProvider, IViewLoader viewLoader, ISearchCasualMatch searchNormalMatch, UserInfo userInfo, IClientButtonBILogger buttonBiLogger, IMainMenuPresenterTree mainMenuPresenterTree, GameModesGUI gameModesGui, IGetLocalPlayerNoviceTrialsTarget getGetLocalPlayerNoviceTrialsTarget, IGetPlayerRemainingNoviceTrials getPlayerRemainingNoviceTrials, ILocalizeKey translation)
		{
			this._logger = logger;
			this._viewProvider = viewProvider;
			this._viewLoader = viewLoader;
			this._searchNormalMatch = searchNormalMatch;
			this._userInfo = userInfo;
			this._buttonBiLogger = buttonBiLogger;
			this._mainMenuPresenterTree = mainMenuPresenterTree;
			this._gameModesGui = gameModesGui;
			this._getGetLocalPlayerNoviceTrialsTarget = getGetLocalPlayerNoviceTrialsTarget;
			this._getPlayerRemainingNoviceTrials = getPlayerRemainingNoviceTrials;
			this._translation = translation;
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.Do<Unit>(this._viewLoader.LoadView("UI_ADD_Training_PopUp"), delegate(Unit _)
			{
				this.InitializeView();
			});
		}

		public IObservable<Unit> ShowAndWaitForConclusion()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._view.Canvas.Enable();
				return Observable.DoOnTerminate<Unit>(Observable.DoOnCancel<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(this._view.ScreenInAnimation.Play(), delegate(Unit _)
				{
					this.EnableButtons();
					this._view.UiNavigationGroupHolder.AddGroup();
				}), (Unit _) => Observable.First<Unit>(Observable.Merge<Unit>(new IObservable<Unit>[]
				{
					this.GoToCasualModeButton(),
					this.GoToTrainingScreenButton(),
					this.GoToPreCustomMatchScreenButton(),
					this.OnCloseButtonClicked()
				}))), delegate()
				{
					ObservableExtensions.Subscribe<Unit>(this.UnloadView());
				}), delegate()
				{
					ObservableExtensions.Subscribe<Unit>(this.UnloadView());
				});
			});
		}

		private IObservable<Unit> UnloadView()
		{
			return this._viewLoader.UnloadView("UI_ADD_Training_PopUp");
		}

		private void InitializeView()
		{
			this._view = this._viewProvider.Provide<ITrainingPopUpView>(null);
			this.DisableButtons();
			this._view.CheckBox.IsOn = false;
			this.UpdateDescriptionLabel();
			this.UpdateMatchCountLabel();
		}

		private void UpdateDescriptionLabel()
		{
			int num = this._getGetLocalPlayerNoviceTrialsTarget.Get();
			this._view.DescriptionLabel.Text = this._translation.GetFormatted("MAIN_MENU_ONBOARDING_TRAINING_DESCRIPTION", TranslationContext.MainMenuGui, new object[]
			{
				num
			});
		}

		private void UpdateMatchCountLabel()
		{
			int num = this._getPlayerRemainingNoviceTrials.Get();
			int num2 = this._getGetLocalPlayerNoviceTrialsTarget.Get();
			int num3 = Math.Max(num2 - num, 0);
			this._view.MatchCountLabel.Text = string.Format("{0}/{1}", num3, num2);
		}

		private IObservable<Unit> GoToCasualModeButton()
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.First<Unit>(this._view.PlayCasualButton.OnClick()), delegate(Unit _)
			{
				this.CheckAndSaveToggleOption();
			}), delegate(Unit _)
			{
				this.DisableButtons();
			}), delegate(Unit _)
			{
				this._buttonBiLogger.LogButtonClick(ButtonName.GameModeNormalPopupV2ConfirmNovice);
			}), delegate(Unit _)
			{
				this._searchNormalMatch.Search();
			}), (Unit _) => this.Hide());
		}

		private IObservable<Unit> GoToTrainingScreenButton()
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.First<Unit>(this._view.PlayTrainingButton.OnClick()), delegate(Unit _)
			{
				this.CheckAndSaveToggleOption();
			}), delegate(Unit _)
			{
				this.DisableButtons();
			}), delegate(Unit _)
			{
				this._buttonBiLogger.LogButtonClick(ButtonName.GameModeNormalPopupV2ConfirmTraining);
			}), delegate(Unit _)
			{
				ObservableExtensions.Subscribe<Unit>(this._mainMenuPresenterTree.PresenterTree.NavigateToNode(this._mainMenuPresenterTree.MainMenuTrainingSelectionNode));
			}), (Unit _) => this.Hide());
		}

		private IObservable<Unit> GoToPreCustomMatchScreenButton()
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.First<Unit>(this._view.CustomMatchButton.OnClick()), delegate(Unit _)
			{
				this.CheckAndSaveToggleOption();
			}), delegate(Unit _)
			{
				this.DisableButtons();
			}), delegate(Unit _)
			{
				this._buttonBiLogger.LogButtonClick(ButtonName.GameModeNormalPopupV2ConfirmCustom);
			}), delegate(Unit _)
			{
				this._gameModesGui.OnClickedCustomMatchFromMainMenu();
			}), (Unit _) => this.Hide());
		}

		private IObservable<Unit> OnCloseButtonClicked()
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.First<Unit>(this._view.CloseButton.OnClick()), delegate(Unit _)
			{
				this.CheckAndSaveToggleOption();
			}), delegate(Unit _)
			{
				this.DisableButtons();
			}), delegate(Unit _)
			{
				this._buttonBiLogger.LogButtonClick(ButtonName.GameModeNormalPopupV2Close);
			}), (Unit _) => this.Hide());
		}

		private IObservable<Unit> Hide()
		{
			this._view.UiNavigationGroupHolder.RemoveGroup();
			return this._view.ScreenOutAnimation.Play();
		}

		private void DisableButtons()
		{
			this._view.PlayCasualButton.IsInteractable = false;
			this._view.PlayTrainingButton.IsInteractable = false;
			this._view.CloseButton.IsInteractable = false;
			this._view.CustomMatchButton.IsInteractable = false;
		}

		private void CheckAndSaveToggleOption()
		{
			this._logger.Debug("Check for player disable training popUp");
			if (this._view.CheckBox.IsOn)
			{
				this._logger.Debug("Saving player disable training popUp");
				ObservableExtensions.Subscribe<PlayerBag>(this.SavePlayerOptionDisablePopUp());
			}
		}

		private void EnableButtons()
		{
			this._view.PlayCasualButton.IsInteractable = true;
			this._view.PlayTrainingButton.IsInteractable = true;
			this._view.CloseButton.IsInteractable = true;
			this._view.CustomMatchButton.IsInteractable = true;
		}

		private IObservable<PlayerBag> SavePlayerOptionDisablePopUp()
		{
			return Observable.DoOnError<PlayerBag>(Observable.Do<PlayerBag>(this.SavePlayerOptionDisablePopUpInCustomWS(), new Action<PlayerBag>(this.OnDisablePopUpSucess)), new Action<Exception>(this.OnDisablePopUpError));
		}

		private void OnDisablePopUpSucess(PlayerBag playerBag)
		{
			this._userInfo.Bag = playerBag;
			this._logger.Info("Disable training popUp saved");
		}

		private void OnDisablePopUpError(Exception obj)
		{
			this._logger.ErrorFormat("Failed to save disable training popUp. Ex: {0} ", new object[]
			{
				obj
			});
		}

		private IObservable<PlayerBag> SavePlayerOptionDisablePopUpInCustomWS()
		{
			return Observable.Select<string, PlayerBag>(SwordfishObservable.FromSwordfishCall<string>(delegate(SwordfishClientApi.ParameterizedCallback<string> onSuccess, SwordfishClientApi.ErrorCallback onError)
			{
				PlayerCustomWS.DisableTrainingPopUp(this._userInfo.UniversalId, onSuccess, onError);
			}), (string result) => (PlayerBag)result);
		}

		private const string SceneName = "UI_ADD_Training_PopUp";

		private IViewProvider _viewProvider;

		private IViewLoader _viewLoader;

		private IMainMenuPresenterTree _mainMenuPresenterTree;

		private readonly GameModesGUI _gameModesGui;

		private readonly IGetLocalPlayerNoviceTrialsTarget _getGetLocalPlayerNoviceTrialsTarget;

		private readonly IGetPlayerRemainingNoviceTrials _getPlayerRemainingNoviceTrials;

		private readonly ILocalizeKey _translation;

		private ISearchCasualMatch _searchNormalMatch;

		private UserInfo _userInfo;

		private IClientButtonBILogger _buttonBiLogger;

		private readonly ILogger<TrainingPopUpPresenter> _logger;

		private ITrainingPopUpView _view;
	}
}
