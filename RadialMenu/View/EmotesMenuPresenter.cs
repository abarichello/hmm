using System;
using System.Collections.Generic;
using System.Linq;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Customization.Infra;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Items.DataTransferObjects;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.QuickChat;
using Hoplon.Input;
using Hoplon.Input.UiNavigation;
using Hoplon.Localization.TranslationTable;
using Hoplon.Reactive;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.RadialMenu.View
{
	public class EmotesMenuPresenter : IEmotesMenuPresenter, IRadialMenuPresenter
	{
		public EmotesMenuPresenter(IViewLoader viewLoader, IViewProvider viewProvider, ICustomizationService customizationService, IMatchPlayers matchPlayers, ILocalizeKey translation, IHudChatPresenter hudChatPresenter, ISendGadgetInputCommand sendGadgetInputCommand, IControllerInputActionPoller controllerInputActionPoller, IGameObservable gameObservable, IInputGetActiveDevicePoller inputGetActiveDevicePoller)
		{
			this._viewLoader = viewLoader;
			this._viewProvider = viewProvider;
			this._customizationService = customizationService;
			this._matchPlayers = matchPlayers;
			this._translation = translation;
			this._hudChatPresenter = hudChatPresenter;
			this._sendGadgetInputCommand = sendGadgetInputCommand;
			this._controllerInputActionPoller = controllerInputActionPoller;
			this._gameObservable = gameObservable;
			this._inputGetActiveDevicePoller = inputGetActiveDevicePoller;
		}

		public IObservable<Unit> Initialize()
		{
			this._spamFilter = new FloodFilter(4, 10, 10);
			return Observable.Do<Unit>(this._viewLoader.LoadView("UI_ADD_RadialMenu"), delegate(Unit _)
			{
				this.InitializeView();
			});
		}

		public void Show()
		{
			this.DisposeHighlightSliceOperation();
			this.EnableNotifier();
			this.RegisterQuadrantChangeNotification();
			this.PlayInAnimation();
			this._view.UiNavigationGroupHolder.AddGroup();
		}

		public void Hide()
		{
			this.DisableNotifier();
			this.DisposeHighlightSliceOperation();
			this.PlayOutAnimation();
			this.PlaySelectionOutAnimation(this._currentQuadrant);
			this._view.UiNavigationGroupHolder.RemoveGroup();
		}

		public void SendSelectedItem()
		{
			this.SendCommand();
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

		private void OnQuadrantChange(RadialSliceChange sliceChange)
		{
			this._currentQuadrant = sliceChange.CurrentSliceIndex;
			if (sliceChange.PreviousSliceIndex > -1)
			{
				this.PlaySelectionOutAnimation(sliceChange.PreviousSliceIndex);
			}
			this.PlaySelectionInAnimation(sliceChange.CurrentSliceIndex);
		}

		public IObservable<Unit> Dispose()
		{
			this.DisableNotifier();
			this.DisposeHighlightSliceOperation();
			return this._viewLoader.UnloadView("UI_ADD_RadialMenu");
		}

		private void SendCommand()
		{
			switch (this._currentQuadrant)
			{
			case 0:
				break;
			case 1:
				this.SendGadgetCommandIfValid(GadgetSlot.EmoteGadget0, 40);
				break;
			case 2:
				this.SendGadgetCommandIfValid(GadgetSlot.EmoteGadget1, 41);
				break;
			case 3:
				this.SendGadgetCommandIfValid(GadgetSlot.EmoteGadget2, 42);
				break;
			case 4:
				this.SendGadgetCommandIfValid(GadgetSlot.EmoteGadget3, 43);
				break;
			default:
				Debug.LogWarningFormat("Unknown RadialMenu quadrant command:{0}", new object[]
				{
					this._currentQuadrant
				});
				break;
			}
		}

		private void SendGadgetCommandIfValid(GadgetSlot slot, PlayerCustomizationSlot customizationSlot)
		{
			CustomizationContent currentPlayerCustomizationContent = this._customizationService.GetCurrentPlayerCustomizationContent();
			Guid guidBySlot = currentPlayerCustomizationContent.GetGuidBySlot(customizationSlot);
			if (guidBySlot == Guid.Empty)
			{
				return;
			}
			if (this._spamFilter.IsSpam(string.Empty, Time.unscaledTime))
			{
				this._hudChatPresenter.AddChatMessage(this._translation.Get("EMOTE_PENALITY_DRAFT", TranslationContext.Hud));
			}
			else
			{
				this._sendGadgetInputCommand.Send(slot);
			}
		}

		private void EnableNotifier()
		{
			this._view.RadialMenuMouseNotifier.enabled = true;
		}

		private void DisableNotifier()
		{
			if (this._view != null && null != this._view.RadialMenuMouseNotifier)
			{
				this._view.RadialMenuMouseNotifier.enabled = false;
			}
		}

		private void RegisterQuadrantChangeNotification()
		{
			this._highlightSlicesOperation = ObservableExtensions.Subscribe<RadialSliceChange>(this._view.RadialMenuMouseNotifier.CurrentSliceChanged, new Action<RadialSliceChange>(this.OnQuadrantChange));
		}

		private void DisposeHighlightSliceOperation()
		{
			if (this._highlightSlicesOperation == null)
			{
				return;
			}
			this._highlightSlicesOperation.Dispose();
			this._highlightSlicesOperation = null;
		}

		private void PlayInAnimation()
		{
			this._view.WindowAnimationIn();
		}

		private void PlayOutAnimation()
		{
			this._view.WindowAnimationOut();
		}

		private void PlaySelectionInAnimation(int index)
		{
			this._view.SelectorGlowIn(index);
			this._view.SpritesheetAnimators[index].StartAnimation();
		}

		private void PlaySelectionOutAnimation(int index)
		{
			for (int i = 0; i < this._view.SpritesheetAnimators.Length; i++)
			{
				this._view.SpritesheetAnimators[i].Stop();
			}
			this._view.SelectorGlowOut(index);
		}

		private void PlayAnimation(Animation anim, string clipName)
		{
			anim.Play(clipName);
		}

		private void InitializeView()
		{
			this.GetViewFromProvider();
			this.PreloadTextures();
		}

		private static bool AllValuesAreTrue(IList<bool> values)
		{
			return values.All((bool value) => value);
		}

		private void GetViewFromProvider()
		{
			this._view = this._viewProvider.Provide<IRadialMenuView>(null);
		}

		private void PreloadTextures()
		{
			CustomizationContent currentPlayerCustomizationContent = this._customizationService.GetCurrentPlayerCustomizationContent();
			this.UpdateEmote(40, currentPlayerCustomizationContent, this._view.SpritesheetAnimators[1]);
			this.UpdateEmote(41, currentPlayerCustomizationContent, this._view.SpritesheetAnimators[2]);
			this.UpdateEmote(42, currentPlayerCustomizationContent, this._view.SpritesheetAnimators[3]);
			this.UpdateEmote(43, currentPlayerCustomizationContent, this._view.SpritesheetAnimators[4]);
		}

		private void UpdateEmote(PlayerCustomizationSlot slot, CustomizationContent customization, ITextureMappingUpdater textureMappingUpdater)
		{
			ItemTypeScriptableObject itemTypeScriptableObjectBySlot = this._customizationService.GetItemTypeScriptableObjectBySlot(slot, customization);
			if (itemTypeScriptableObjectBySlot == null)
			{
				textureMappingUpdater.ChangeVisibility(false);
				return;
			}
			textureMappingUpdater.ChangeVisibility(true);
			EmoteItemTypeComponent component = itemTypeScriptableObjectBySlot.GetComponent<EmoteItemTypeComponent>();
			textureMappingUpdater.TryToLoadAsset(component.spriteSheetName);
			textureMappingUpdater.StartAnimation();
			textureMappingUpdater.Stop();
		}

		private const string SceneName = "UI_ADD_RadialMenu";

		private const string RadialMenuIn = "radialMenu_in";

		private const string RadialMenuOut = "radialMenu_out";

		private const string SelectionIn = "radialSelector_in";

		private const string SelectionOut = "radialSelector_out";

		private readonly IViewLoader _viewLoader;

		private readonly IViewProvider _viewProvider;

		private readonly ICustomizationService _customizationService;

		private readonly IMatchPlayers _matchPlayers;

		private readonly ILocalizeKey _translation;

		private readonly IHudChatPresenter _hudChatPresenter;

		private readonly ISendGadgetInputCommand _sendGadgetInputCommand;

		private readonly IControllerInputActionPoller _controllerInputActionPoller;

		private readonly IGameObservable _gameObservable;

		private readonly IInputGetActiveDevicePoller _inputGetActiveDevicePoller;

		private ISpamFilter _spamFilter;

		private IRadialMenuView _view;

		private int _currentQuadrant;

		private IDisposable _highlightSlicesOperation;
	}
}
