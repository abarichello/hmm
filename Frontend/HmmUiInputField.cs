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
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	[AddComponentMenu("UI/HMM/HmmUiInputField")]
	public class HmmUiInputField : InputField
	{
		private UiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		protected override void Start()
		{
			base.Start();
			this.UpdateCachedString();
			if (null != this.UiNavigationGroupHolder)
			{
				this._inputCancelDownDisposable = ObservableExtensions.Subscribe<Unit>(this.UiNavigationGroupHolder.ObserveInputCancelDown(), delegate(Unit _)
				{
					this.CancelInputEdit();
				});
			}
			if (null != this._gamepadShortcutImage && Application.isPlaying)
			{
				this._inputActiveDeviceChangeNotifierDisposable = ObservableExtensions.Subscribe<InputDevice>(this._inputActiveDeviceChangeNotifier.GetAndObserveActiveDeviceChange(), delegate(InputDevice inputDevice)
				{
					this.UpdateGamepadShortcut();
					this.UpdatePasteFromClipboardDefaultText();
				});
			}
			if (null != this._pasteUiNavigationContextInputNotifier)
			{
				this._pasteUiNavigationContextInputNotifierDisposable = ObservableExtensions.Subscribe<UiNavigationInputCode>(this._pasteUiNavigationContextInputNotifier.ObserveInputDown(), new Action<UiNavigationInputCode>(this.TryToPaste));
			}
		}

		public override void OnSelect(BaseEventData eventData)
		{
			this._isSelected = true;
			this.UpdatePasteFromClipboardDefaultText();
			if (!this._enableUiNavigation)
			{
				base.OnSelect(eventData);
			}
			else
			{
				this.EmulatePointerEnter();
			}
		}

		public override void OnDeselect(BaseEventData eventData)
		{
			this._isSelected = false;
			base.OnDeselect(eventData);
			if (this._enableUiNavigation)
			{
				this.EmulatePointerExit();
			}
			this.TryToRemoveHighPriorityGroup();
		}

		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			this.TryToAddHighPriorityGroup();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			this._isDestroyed = true;
			this.TryToRemoveHighPriorityGroup();
			if (this._inputCancelDownDisposable != null)
			{
				this._inputCancelDownDisposable.Dispose();
				this._inputCancelDownDisposable = null;
			}
			if (this._inputActiveDeviceChangeNotifierDisposable != null)
			{
				this._inputActiveDeviceChangeNotifierDisposable.Dispose();
				this._inputActiveDeviceChangeNotifierDisposable = null;
			}
			if (this._pasteUiNavigationContextInputNotifierDisposable != null)
			{
				this._pasteUiNavigationContextInputNotifierDisposable.Dispose();
				this._pasteUiNavigationContextInputNotifierDisposable = null;
			}
		}

		private void TryToAddHighPriorityGroup()
		{
			if (null == this.UiNavigationGroupHolder)
			{
				return;
			}
			if (!Platform.Current.HasPhysicalKeyboard())
			{
				this.CancelInputEdit();
				this.ShowVirtualKeyboard();
			}
			else
			{
				this.UiNavigationGroupHolder.AddHighPriorityGroup();
				this._isFocused = true;
				this.UpdateGamepadShortcut();
			}
		}

		private void ShowVirtualKeyboard()
		{
			if (this._isVirtualKeyboardOpen)
			{
				return;
			}
			this._isVirtualKeyboardOpen = true;
			this._virtualKeyboardDisposable = ObservableExtensions.Subscribe<string>(Observable.DoOnError<string>(Observable.Do<string>(Platform.Current.ShowVirtualKeyboard(base.contentType, base.text), new Action<string>(this.OnVirtualKeyboardClose)), delegate(Exception _)
			{
				this.OnVirtualKeyboardError();
			}));
		}

		private void OnVirtualKeyboardClose(string finalText)
		{
			if (finalText != null)
			{
				base.text = finalText;
			}
			this._isVirtualKeyboardOpen = false;
			this._virtualKeyboardCloseSubject.OnNext(finalText);
		}

		private void OnVirtualKeyboardError()
		{
			this._isVirtualKeyboardOpen = false;
			HmmUiInputField.Log.Error("Failed to open virtual keyboard");
		}

		private void CancelInputEdit()
		{
			this.OnDeselect(HmmUiInputField.GetLeftMousePointerData());
			this.EmulatePointerEnter();
		}

		private void TryToRemoveHighPriorityGroup()
		{
			this.TryToDisposeVirtualKeyboard();
			this.TryToRemoveUiNavigationFocus();
		}

		private void TryToRemoveUiNavigationFocus()
		{
			if (null != this.UiNavigationGroupHolder)
			{
				this.UiNavigationGroupHolder.RemoveHighPriorityGroup();
				this._isFocused = false;
				this.UpdateGamepadShortcut();
			}
		}

		private void TryToDisposeVirtualKeyboard()
		{
			if (!this._isVirtualKeyboardOpen && this._virtualKeyboardDisposable != null)
			{
				this._virtualKeyboardDisposable.Dispose();
				this._virtualKeyboardDisposable = null;
			}
		}

		private void EmulatePointerEnter()
		{
			this._isPointerEnter = true;
			this.OnPointerEnter(HmmUiInputField.GetLeftMousePointerData());
			this.UpdateGamepadShortcut();
		}

		private void EmulatePointerExit()
		{
			this._isPointerEnter = false;
			this.OnPointerExit(HmmUiInputField.GetLeftMousePointerData());
			this.UpdateGamepadShortcut();
		}

		private static PointerEventData GetLeftMousePointerData()
		{
			return new PointerEventData(EventSystem.current)
			{
				button = 0
			};
		}

		private void UpdateGamepadShortcut()
		{
			if (this._isDestroyed || null == this._gamepadShortcutImage || !Application.isPlaying)
			{
				return;
			}
			bool flag = this.IsJoystickActive();
			bool flag2 = (this._isFocused || this._isPointerEnter) && flag;
			if (flag2)
			{
				ControllerInputActions controllerInputActions = (!this._isFocused) ? 33 : 34;
				ISprite sprite;
				string text;
				this._inputTranslation.TryToGetInputActionJoystickAssetOrFallbackToTranslation(controllerInputActions, ref sprite, ref text);
				this._gamepadShortcutImage.sprite = (sprite as UnitySprite).GetSprite();
			}
			this._gamepadShortcutImage.gameObject.SetActive(flag2);
		}

		public IObservable<string> ObserveVirtualKeyboardClose()
		{
			return this._virtualKeyboardCloseSubject;
		}

		private void UpdatePasteFromClipboardDefaultText()
		{
			if (!this._showPasteFromClipboardInfo || null == this._pasteUiNavigationContextInputNotifier)
			{
				return;
			}
			Text text = base.placeholder as Text;
			if (text == null)
			{
				return;
			}
			if (this._isSelected && this.IsJoystickActive() && Clipboard.HasClipboardContent())
			{
				string text2 = text.text;
				string pasteText = this.GetPasteText();
				if (text2 == pasteText)
				{
					return;
				}
				text.text = pasteText;
				this._cachedDefaultText = text2;
			}
			else
			{
				text.text = this._cachedDefaultText;
			}
		}

		private void UpdateCachedString()
		{
			Text text = base.placeholder as Text;
			if (text == null)
			{
				return;
			}
			this._cachedDefaultText = text.text;
		}

		private string GetPasteText()
		{
			if (HmmUiInputField._pasteText == null)
			{
				HmmUiInputField._pasteText = string.Format(Language.Get("HINT_FEEDBACK_PASTE_TEXT", TranslationContext.MainMenuGui), this._inputTranslation.GetInputActionActiveDeviceTranslation(42));
			}
			return HmmUiInputField._pasteText;
		}

		private void TryToPaste(UiNavigationInputCode inputCode)
		{
			if (inputCode == 11 && Clipboard.HasClipboardContent())
			{
				base.text = Clipboard.GetClipboard();
			}
		}

		private bool IsJoystickActive()
		{
			return this._inputGetActiveDevicePoller.GetActiveDevice() == 3;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HmmUiInputField));

		[SerializeField]
		private bool _enableUiNavigation;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private Image _gamepadShortcutImage;

		[SerializeField]
		private bool _showPasteFromClipboardInfo;

		[SerializeField]
		private UiNavigationContextInputNotifier _pasteUiNavigationContextInputNotifier;

		[InjectOnClient]
		private IInputTranslation _inputTranslation;

		[InjectOnClient]
		private IInputActiveDeviceChangeNotifier _inputActiveDeviceChangeNotifier;

		[InjectOnClient]
		private IInputGetActiveDevicePoller _inputGetActiveDevicePoller;

		private IDisposable _inputCancelDownDisposable;

		private IDisposable _inputActiveDeviceChangeNotifierDisposable;

		private IDisposable _virtualKeyboardDisposable;

		private IDisposable _pasteUiNavigationContextInputNotifierDisposable;

		private bool _isFocused;

		private bool _isPointerEnter;

		private bool _isDestroyed;

		private bool _isVirtualKeyboardOpen;

		private bool _isSelected;

		private string _cachedDefaultText;

		private static string _pasteText;

		private readonly Subject<string> _virtualKeyboardCloseSubject = new Subject<string>();
	}
}
