using System;
using ClientAPI;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Training.View;
using Hoplon.Logging;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Training.Presenter
{
	public class TrainingPopUpPresenterV3 : ITrainingPopUpPresenterV3, ITrainingPopUpPresenter
	{
		public TrainingPopUpPresenterV3(ILogger<TrainingPopUpPresenterV3> logger)
		{
			this._logger = logger;
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.Do<Unit>(this._viewLoader.LoadView("UI_ADD_Training_PopUp_V3"), delegate(Unit _)
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
					this.GoBackButton(),
					this.GoToTrainingScreenButton()
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
			return this._viewLoader.UnloadView("UI_ADD_Training_PopUp_V3");
		}

		private void InitializeView()
		{
			this._view = this._viewProvider.Provide<ITrainingPopUpViewV3>(null);
			this.DisableButtons();
		}

		private IObservable<Unit> GoBackButton()
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.First<Unit>(this._view.BackButton.OnClick()), delegate(Unit _)
			{
				this.RemovePopUpFlagFromInventory();
			}), delegate(Unit _)
			{
				this.DisableButtons();
			}), delegate(Unit _)
			{
				this._buttonBILogger.LogButtonClick(ButtonName.TrainingPopupBack);
			}), delegate(Unit _)
			{
				this._mainMenuInitialization.NodeToGo = MainMenuNode.None;
			}), (Unit _) => this.Hide());
		}

		private IObservable<Unit> GoToTrainingScreenButton()
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.First<Unit>(this._view.ConfirmButton.OnClick()), delegate(Unit _)
			{
				this.RemovePopUpFlagFromInventory();
			}), delegate(Unit _)
			{
				this.DisableButtons();
			}), delegate(Unit _)
			{
				this._buttonBILogger.LogButtonClick(ButtonName.TrainingPopupAccept);
			}), delegate(Unit _)
			{
				this._mainMenuInitialization.NodeToGo = MainMenuNode.TrainingScreen;
			}), (Unit _) => this.Hide());
		}

		private IObservable<Unit> Hide()
		{
			this._view.UiNavigationGroupHolder.RemoveGroup();
			return this._view.ScreenOutAnimation.Play();
		}

		private void DisableButtons()
		{
			this._view.BackButton.IsInteractable = false;
			this._view.ConfirmButton.IsInteractable = false;
		}

		private void RemovePopUpFlagFromInventory()
		{
			this._logger.Debug("Check for player disable training popUp");
			this._logger.Debug("Saving player disable training popUp");
			ObservableExtensions.Subscribe<PlayerBag>(this.SavePlayerOptionDisablePopUp());
		}

		private void EnableButtons()
		{
			this._view.BackButton.IsInteractable = true;
			this._view.ConfirmButton.IsInteractable = true;
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

		private const string SceneName = "UI_ADD_Training_PopUp_V3";

		[InjectOnClient]
		private IViewProvider _viewProvider;

		[InjectOnClient]
		private IViewLoader _viewLoader;

		[Inject]
		private IMainMenuPresenterTree _mainMenuPresenterTree;

		[InjectOnClient]
		private UserInfo _userInfo;

		[Inject]
		private IClientButtonBILogger _buttonBILogger;

		[Inject]
		private IMainMenuInitialization _mainMenuInitialization;

		private readonly ILogger<TrainingPopUpPresenterV3> _logger;

		private ITrainingPopUpViewV3 _view;
	}
}
