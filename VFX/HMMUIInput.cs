using System;
using HeavymetalMachines.Infra.Clipboard;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.ContextInputNotifier;
using Pocketverse;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class HMMUIInput : UIInput
	{
		private IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public override void Start()
		{
			base.Start();
			this.AddForceFullCapsValidation();
			if (this.UiNavigationGroupHolder == null)
			{
				return;
			}
			this._inputCancelDownDisposable = ObservableExtensions.Subscribe<Unit>(this.UiNavigationGroupHolder.ObserveInputCancelDown(), delegate(Unit _)
			{
				this.InputCancelSelection();
			});
			this._activeDeviceDisposable = ObservableExtensions.Subscribe<InputDevice>(this._inputActiveDeviceChangeNotifier.ObserveActiveDeviceChange(), delegate(InputDevice device)
			{
				this.UpdateShortcut();
				this.UpdatePasteFromClipboardDefaultText();
			});
			if (null != this._pasteUiNavigationContextInputNotifier)
			{
				this._pasteUiNavigationContextInputNotifierDisposable = ObservableExtensions.Subscribe<UiNavigationInputCode>(this._pasteUiNavigationContextInputNotifier.ObserveInputDown(), new Action<UiNavigationInputCode>(this.TryToPaste));
			}
		}

		private void AddForceFullCapsValidation()
		{
			if (this._forceFullCaps)
			{
				this.onValidate = (UIInput.OnValidate)Delegate.Combine(this.onValidate, new UIInput.OnValidate((string _, int __, char character) => char.ToUpper(character)));
			}
		}

		private void TryToPaste(UiNavigationInputCode inputCode)
		{
			if (inputCode == 11 && Clipboard.HasClipboardContent())
			{
				base.value = Clipboard.GetClipboard();
			}
		}

		private void UpdatePasteFromClipboardDefaultText()
		{
			if (!this._showPasteFromClipboardInfo || null == this._pasteUiNavigationContextInputNotifier)
			{
				return;
			}
			if (this._isHovered && this.IsJoystickActive() && Clipboard.HasClipboardContent())
			{
				string defaultText = base.defaultText;
				string pasteText = this.GetPasteText();
				if (defaultText == pasteText)
				{
					return;
				}
				base.defaultText = pasteText;
				this._cachedDefaultText = defaultText;
			}
			else
			{
				base.defaultText = this._cachedDefaultText;
			}
		}

		private string GetPasteText()
		{
			if (HMMUIInput._pasteText == null)
			{
				HMMUIInput._pasteText = string.Format(Language.Get("HINT_FEEDBACK_PASTE_TEXT", TranslationContext.MainMenuGui), this._inputTranslation.GetInputActionActiveDeviceTranslation(42));
			}
			return HMMUIInput._pasteText;
		}

		protected override void OnDefaultTextSet(string newDefaultText)
		{
			this._cachedDefaultText = newDefaultText;
		}

		private void InputCancelSelection()
		{
			UICamera.selectedObject = null;
			UICamera.Notify(base.gameObject, "OnHover", true);
		}

		protected override void OnSelect(bool isSelected)
		{
			base.OnSelect(isSelected);
			if (this.UiNavigationGroupHolder == null)
			{
				return;
			}
			this._isSelected = isSelected;
			if (isSelected)
			{
				if (!Platform.Current.HasPhysicalKeyboard())
				{
					this.InputCancelSelection();
					this.ShowVirtualKeyboard();
				}
				else
				{
					this.UiNavigationGroupHolder.AddHighPriorityGroup();
				}
			}
			else
			{
				this.TryToDisposeVirtualKeyboard();
				this.UiNavigationGroupHolder.RemoveHighPriorityGroup();
			}
			this.UpdateShortcut();
			this.SetActiveHighlight(isSelected);
			this._onIsSelectedSubject.OnNext(isSelected);
		}

		private void ShowVirtualKeyboard()
		{
			if (this._isVirtualKeyboardOpen)
			{
				return;
			}
			this._isVirtualKeyboardOpen = true;
			this._virtualKeyboardDisposable = ObservableExtensions.Subscribe<string>(Observable.DoOnError<string>(Observable.Do<string>(Platform.Current.ShowVirtualKeyboard(0, base.value), new Action<string>(this.OnVirtualKeyboardClose)), delegate(Exception _)
			{
				this.OnVirtualKeyboardError();
			}));
		}

		private void OnVirtualKeyboardClose(string finalText)
		{
			if (finalText != null)
			{
				base.value = finalText;
			}
			this._isVirtualKeyboardOpen = false;
			this._virtualKeyboardCloseSubject.OnNext(finalText);
		}

		private void OnVirtualKeyboardError()
		{
			this._isVirtualKeyboardOpen = false;
			HMMUIInput.Log.Error("Failed to open virtual keyboard");
		}

		private void TryToDisposeVirtualKeyboard()
		{
			if (!this._isVirtualKeyboardOpen && this._virtualKeyboardDisposable != null)
			{
				this._virtualKeyboardDisposable.Dispose();
				this._virtualKeyboardDisposable = null;
			}
		}

		private void SetActiveHighlight(bool active)
		{
			if (this._highlightObjectOnFocus == null)
			{
				return;
			}
			this._highlightObjectOnFocus.SetActive(active);
		}

		private void UpdateShortcut()
		{
			if (null == this._gamepadCancelFocusShortcutImage)
			{
				return;
			}
			bool flag = (this._isSelected || this._isHovered) && this._inputGetActiveDevicePoller.GetActiveDevice() == 3;
			if (flag)
			{
				this.TryToSetupShortcutIcon();
			}
			this._gamepadCancelFocusShortcutImage.gameObject.SetActive(flag);
		}

		private void TryToSetupShortcutIcon()
		{
			if (null == this._gamepadCancelFocusShortcutImage)
			{
				return;
			}
			ControllerInputActions controllerInputActions = (!this._isSelected) ? 33 : 34;
			ISprite sprite;
			string text;
			this._inputTranslation.TryToGetInputActionJoystickAssetOrFallbackToTranslation(controllerInputActions, ref sprite, ref text);
			this._gamepadCancelFocusShortcutImage.sprite2D = (sprite as UnitySprite).GetSprite();
		}

		public IObservable<bool> ObserveChatGetSelectOrUnselected()
		{
			return this._onIsSelectedSubject;
		}

		private void OnDestroy()
		{
			if (this._inputCancelDownDisposable != null)
			{
				this._inputCancelDownDisposable.Dispose();
				this._inputCancelDownDisposable = null;
			}
			if (this._activeDeviceDisposable != null)
			{
				this._activeDeviceDisposable.Dispose();
				this._activeDeviceDisposable = null;
			}
			if (this._pasteUiNavigationContextInputNotifierDisposable != null)
			{
				this._pasteUiNavigationContextInputNotifierDisposable.Dispose();
				this._pasteUiNavigationContextInputNotifierDisposable = null;
			}
		}

		protected void OnHover(bool isOver)
		{
			this._isHovered = isOver;
			this.UpdateShortcut();
			this.UpdatePasteFromClipboardDefaultText();
		}

		public IObservable<string> ObserveVirtualKeyboardClose()
		{
			return this._virtualKeyboardCloseSubject;
		}

		private bool IsJoystickActive()
		{
			return this._inputGetActiveDevicePoller.GetActiveDevice() == 3;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HMMUIInput));

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private GameObjectActivatable _highlightObjectOnFocus;

		[SerializeField]
		private UI2DSprite _gamepadCancelFocusShortcutImage;

		[SerializeField]
		private bool _showPasteFromClipboardInfo;

		[SerializeField]
		private bool _forceFullCaps;

		[SerializeField]
		private UiNavigationContextInputNotifier _pasteUiNavigationContextInputNotifier;

		[InjectOnClient]
		private IInputActiveDeviceChangeNotifier _inputActiveDeviceChangeNotifier;

		[InjectOnClient]
		private IInputGetActiveDevicePoller _inputGetActiveDevicePoller;

		[InjectOnClient]
		private IInputTranslation _inputTranslation;

		private IDisposable _inputCancelDownDisposable;

		private IDisposable _activeDeviceDisposable;

		private IDisposable _virtualKeyboardDisposable;

		private IDisposable _pasteUiNavigationContextInputNotifierDisposable;

		private bool _isSelected;

		private bool _isHovered;

		private bool _isVirtualKeyboardOpen;

		private readonly Subject<bool> _onIsSelectedSubject = new Subject<bool>();

		private readonly Subject<string> _virtualKeyboardCloseSubject = new Subject<string>();

		private string _cachedDefaultText;

		private static string _pasteText;
	}
}
