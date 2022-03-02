using System;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.News.Presenting
{
	public class NewsMainButtonShortcutsView : MonoBehaviour, INewsMainButtonShortcutsView
	{
		public IActivatable ShowShortcutActivatable
		{
			get
			{
				return this._showShortcutActivatable;
			}
		}

		public IActivatable HideShortcutActivatable
		{
			get
			{
				return this._hideShortcutActivatable;
			}
		}

		public void UpdateShortcuts()
		{
			this.UpdateShortcut(45, this._showShortcutImage);
			this.UpdateShortcut(34, this._hideShortcutImage);
		}

		private void UpdateShortcut(ControllerInputActions action, UnityImage image)
		{
			ISprite sprite;
			string text;
			if (this._inputTranslation.TryToGetInputActionJoystickAssetOrFallbackToTranslation(action, ref sprite, ref text))
			{
				image.Sprite = sprite;
			}
		}

		private void Awake()
		{
			this._viewProvider.Bind<INewsMainButtonShortcutsView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<INewsMainButtonShortcutsView>(null);
		}

		[SerializeField]
		private GameObjectActivatable _showShortcutActivatable;

		[SerializeField]
		private GameObjectActivatable _hideShortcutActivatable;

		[SerializeField]
		private UnityImage _showShortcutImage;

		[SerializeField]
		private UnityImage _hideShortcutImage;

		[Inject]
		private IViewProvider _viewProvider;

		[Inject]
		private IInputTranslation _inputTranslation;
	}
}
