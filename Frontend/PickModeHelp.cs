using System;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input;
using Pocketverse;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class PickModeHelp : GameHubBehaviour
	{
		private void Awake()
		{
			if (!GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
			}
		}

		private void Start()
		{
			this._activeDeviceChangeNotifierDisposable = ObservableExtensions.Subscribe<InputDevice>(this._inputActiveDeviceChangeNotifier.GetAndObserveActiveDeviceChange(), new Action<InputDevice>(this.UpdateHelpShortcuts));
		}

		private void OnDestroy()
		{
			if (this._activeDeviceChangeNotifierDisposable != null)
			{
				this._activeDeviceChangeNotifierDisposable.Dispose();
			}
		}

		private void UpdateHelpShortcuts(InputDevice activeDevice)
		{
			ISprite sprite;
			string text;
			if (activeDevice == 3)
			{
				this.buttonBorderGameObject.SetActive(false);
				this.keyboardShortcutLabel.gameObject.SetActive(false);
				this.gamepadShortcutSprite.gameObject.SetActive(true);
				this._inputTranslation.TryToGetInputActionJoystickAssetOrFallbackToTranslation(0, ref sprite, ref text);
				this.gamepadShortcutSprite.sprite2D = (sprite as UnitySprite).GetSprite();
				return;
			}
			this.buttonBorderGameObject.SetActive(true);
			this.keyboardShortcutLabel.gameObject.SetActive(true);
			this.gamepadShortcutSprite.gameObject.SetActive(false);
			this._inputTranslation.TryToGetInputActionKeyboardMouseAssetOrFallbackToTranslation(0, ref sprite, ref text);
			this.keyboardShortcutLabel.Text = text;
		}

		private const int HelpInputActionId = 0;

		[SerializeField]
		private UI2DSprite gamepadShortcutSprite;

		[SerializeField]
		private UILabel keyboardShortcutLabel;

		[SerializeField]
		private GameObject buttonBorderGameObject;

		[InjectOnClient]
		private IInputActiveDeviceChangeNotifier _inputActiveDeviceChangeNotifier;

		[InjectOnClient]
		private IInputTranslation _inputTranslation;

		private IDisposable _activeDeviceChangeNotifierDisposable;
	}
}
