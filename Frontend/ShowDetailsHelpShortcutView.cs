using System;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class ShowDetailsHelpShortcutView : MonoBehaviour
	{
		private void Start()
		{
			this._inputActiveDeviceChangeNotifierDisposable = ObservableExtensions.Subscribe<InputDevice>(this._inputActiveDeviceChangeNotifier.GetAndObserveActiveDeviceChange(), delegate(InputDevice inputDevice)
			{
				this.UpdateShortcut();
			});
		}

		private void OnDestroy()
		{
			if (this._inputActiveDeviceChangeNotifierDisposable != null)
			{
				this._inputActiveDeviceChangeNotifierDisposable.Dispose();
			}
		}

		private void UpdateShortcut()
		{
			ISprite sprite;
			string text;
			bool flag = this._inputTranslation.TryToGetInputActionActiveDeviceAssetOrFallbackToTranslation(0, ref sprite, ref text);
			this._shortcutLabel.gameObject.SetActive(!flag);
			this._gamepadSprite.gameObject.SetActive(flag);
			this._shortcutLabel.text = text;
			this._gamepadSprite.sprite2D = ((sprite == null) ? null : (sprite as UnitySprite).GetSprite());
		}

		[Inject]
		private IInputActiveDeviceChangeNotifier _inputActiveDeviceChangeNotifier;

		[Inject]
		private IInputTranslation _inputTranslation;

		[SerializeField]
		private UI2DSprite _gamepadSprite;

		[SerializeField]
		private UILabel _shortcutLabel;

		private IDisposable _inputActiveDeviceChangeNotifierDisposable;
	}
}
