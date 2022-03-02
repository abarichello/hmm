using System;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.RadialMenu.View;
using Hoplon.Input.UiNavigation;
using Hoplon.Logging;
using Hoplon.Reactive;
using UniRx;

namespace HeavyMetalMachines.QuickChat
{
	public class QuickChatMenuPresenter : IQuickChatMenuPresenter, IRadialMenuPresenter
	{
		public QuickChatMenuPresenter(IViewLoader viewLoader, IViewProvider viewProvider, ISendGadgetInputCommand sendGadgetInputCommand, IControllerInputActionPoller controllerInputActionPoller, IGameObservable gameObservable, ILogger<QuickChatMenuPresenter> logger)
		{
			this._viewLoader = viewLoader;
			this._viewProvider = viewProvider;
			this._sendGadgetInputCommand = sendGadgetInputCommand;
			this._controllerInputActionPoller = controllerInputActionPoller;
			this._gameObservable = gameObservable;
			this._logger = logger;
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.Do<Unit>(this._viewLoader.LoadView("UI_ADD_QuickChatMenu"), delegate(Unit _)
			{
				this.InitializeView();
			});
		}

		public IObservable<Unit> Dispose()
		{
			return this._viewLoader.UnloadView("UI_ADD_QuickChatMenu");
		}

		public void Show()
		{
			this._view.RadialMenuNotifier.Enable();
			this._view.ContainerAnimator.SetBoolean("active", true);
			this._view.UiNavigationGroupHolder.AddGroup();
		}

		public void Hide()
		{
			this._view.HideAllHighlights();
			this._view.RadialMenuNotifier.Disable();
			this._view.ContainerAnimator.SetBoolean("active", false);
			this._view.UiNavigationGroupHolder.RemoveGroup();
		}

		public void SendSelectedItem()
		{
			this.ExecuteQuickChat();
		}

		public IObservable<Unit> OnConfirmed()
		{
			return Observable.AsUnitObservable<UiNavigationInputCode>(Observable.Where<UiNavigationInputCode>(this._view.UiNavigationContextInputNotifier.ObserveInputDown(), (UiNavigationInputCode inputCode) => inputCode == 0));
		}

		public IObservable<Unit> OnCanceled()
		{
			return Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				this.OnCancelPressed(),
				this.OnPerformedCancelAction()
			});
		}

		private IObservable<Unit> OnCancelPressed()
		{
			return Observable.AsUnitObservable<UiNavigationInputCode>(Observable.Where<UiNavigationInputCode>(this._view.UiNavigationContextInputNotifier.ObserveInputDown(), (UiNavigationInputCode inputCode) => inputCode == 1));
		}

		private IObservable<Unit> OnPerformedCancelAction()
		{
			return Observable.AsUnitObservable<float>(Observable.Where<float>(this._gameObservable.EveryFrame(), (float _) => this.HasCancelActionInputDown()));
		}

		private bool HasCancelActionInputDown()
		{
			return this._controllerInputActionPoller.GetButtonDown(6) || this._controllerInputActionPoller.GetButtonDown(7) || this._controllerInputActionPoller.GetButtonDown(5) || this._controllerInputActionPoller.GetButtonDown(15);
		}

		private void InitializeView()
		{
			this._view = this._viewProvider.Provide<IQuickChatMenuPresenterView>(null);
			ObservableExtensions.Subscribe<RadialSliceChange>(Observable.Do<RadialSliceChange>(this._view.RadialMenuNotifier.CurrentSliceChanged, new Action<RadialSliceChange>(this.ChangeSliceHighlight)));
		}

		private void ChangeSliceHighlight(RadialSliceChange sliceChange)
		{
			if (sliceChange.PreviousSliceIndex >= 0)
			{
				this._view.HideHighlight(sliceChange.PreviousSliceIndex);
			}
			this._view.ShowHighlight(sliceChange.CurrentSliceIndex);
			this._lastSelectedSliceIndex = sliceChange.CurrentSliceIndex;
		}

		private void ExecuteQuickChat()
		{
			if (this._lastSelectedSliceIndex <= 0)
			{
				return;
			}
			int num = this._lastSelectedSliceIndex - 1;
			GadgetSlot slot = GadgetSlot.QuickChat00 + num;
			this._sendGadgetInputCommand.Send(slot);
		}

		private const string SceneName = "UI_ADD_QuickChatMenu";

		private readonly IViewLoader _viewLoader;

		private readonly IViewProvider _viewProvider;

		private readonly ISendGadgetInputCommand _sendGadgetInputCommand;

		private readonly IControllerInputActionPoller _controllerInputActionPoller;

		private readonly IGameObservable _gameObservable;

		private readonly ILogger<QuickChatMenuPresenter> _logger;

		private IQuickChatMenuPresenterView _view;

		private int _lastSelectedSliceIndex = -1;
	}
}
