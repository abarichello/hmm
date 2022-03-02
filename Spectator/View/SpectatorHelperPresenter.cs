using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Presenting;
using Hoplon.Input;
using UniRx;

namespace HeavyMetalMachines.Spectator.View
{
	public class SpectatorHelperPresenter : ISpectatorHelperPresenter, IHudWindow
	{
		public void Initialize(ISpectatorHelperView view)
		{
			this._view = view;
			this._showing = false;
			this._view.Canvas.IsActive = false;
			ObservableExtensions.Subscribe<Unit>(this._view.CloseButton.OnClick(), delegate(Unit _)
			{
				this.Remove();
			});
			this._windowManager.OnHelp += this.ToggleView;
			this._disposable = ObservableExtensions.Subscribe<InputDevice>(Observable.Do<InputDevice>(this._activeDeviceChangeNotifier.GetAndObserveActiveDeviceChange(), delegate(InputDevice device)
			{
				this._isGamepad = (device == 3);
			}));
		}

		public void Dispose()
		{
			this._windowManager.OnHelp -= this.ToggleView;
			this._disposable.Dispose();
			this._disposable = null;
		}

		private void ToggleView()
		{
			if (!this._showing)
			{
				this._windowManager.Push(this);
			}
			else
			{
				this._windowManager.Remove(this);
			}
		}

		private void Show()
		{
			this._showing = true;
			this._view.PlayInAnimation();
			this._view.GamepadInputGroup.SetActive(this._isGamepad);
			this._view.KeyboardInputGroup.SetActive(!this._isGamepad);
			this.UpdateGamepdImages();
		}

		private void UpdateGamepdImages()
		{
			if (!this._isGamepad)
			{
				return;
			}
			this.SetSpriteImage(this._view.SetBombSelectionGampepadSprite, this._cameraConfigProvider.ToggleFocusOnBombGamepadAction);
			this.SetSpriteImage(this._view.SetCenterArenaFixedCameraGampepadSprite, this._cameraConfigProvider.GetFixedCamerasInputAction(SpectatorCameraGroupType.Center));
			this.SetSpriteImage(this._view.SetFreeCameraMovementGampepadSprite, this._cameraConfigProvider.MoveFreeCameraHorizontalGamepadAxisAction);
			this.SetSpriteImage(this._view.SetNextMachineSelectionGampepadSprite, this._cameraConfigProvider.NextCharacterFocusGamepadAction);
			this.SetSpriteImage(this._view.SetOrbitalCameraMovementGampepadSprite, this._cameraConfigProvider.MoveOrbitalCameraHorizontalGamepadAxisAction);
			this.SetSpriteImage(this._view.SetPreviousMachineSelectionGampepadSprite, this._cameraConfigProvider.PreviousCharacterFocusGamepadAction);
			this.SetSpriteImage(this._view.SetTeamBlueFixedCameraGampepadSprite, this._cameraConfigProvider.GetFixedCamerasInputAction(SpectatorCameraGroupType.BlueTeam));
			this.SetSpriteImage(this._view.SetTeamRedFixedCameraGampepadSprite, this._cameraConfigProvider.GetFixedCamerasInputAction(SpectatorCameraGroupType.RedTeam));
			this.SetSpriteImage(this._view.SetPreviousMachineSelectionGampepadSprite, this._cameraConfigProvider.PreviousCharacterFocusGamepadAction);
			this.SetSpriteImage(this._view.SetToogleFreeCameraGampepadSprite, this._cameraConfigProvider.ToggleFreeCameraGamepadAction);
			this.SetSpriteImage(this._view.SetToogleOrbitalCameraGampepadSprite, this._cameraConfigProvider.ToggleOrbitalCameraGamepadAction);
			this.SetSpriteImage(this._view.SetZoomInGampepadSprite, this._cameraConfigProvider.ZoomInGamepadAction);
			this.SetSpriteImage(this._view.SetZoomOutGampepadSprite, this._cameraConfigProvider.ZoomOutGamepadAction);
		}

		private void SetSpriteImage(ISpriteImage spriteImage, ControllerInputActions action)
		{
			ISprite sprite;
			string text;
			if (this._inputTranslation.TryToGetInputActionJoystickAssetOrFallbackToTranslation(action, ref sprite, ref text))
			{
				spriteImage.Sprite = sprite;
			}
		}

		private void Remove()
		{
			this._windowManager.Remove(this);
		}

		private void Hide()
		{
			this._showing = false;
			this._view.PlayOutAnimation();
			this._view.GamepadInputGroup.SetActive(false);
			this._view.KeyboardInputGroup.SetActive(false);
		}

		public bool CanBeHiddenByEscKey()
		{
			return true;
		}

		public void ChangeWindowVisibility(bool targetVisibleState)
		{
			if (targetVisibleState)
			{
				this.Show();
			}
			else
			{
				this.Hide();
			}
		}

		public bool IsWindowVisible()
		{
			return this._showing;
		}

		public int GetDepth()
		{
			return this._view.Canvas.SortOrder;
		}

		public bool CanOpen()
		{
			return !this._windowManager.IsWindowVisible<OptionsWindow>() && !this._windowManager.IsWindowVisible<EscMenuGui>();
		}

		public bool IsStackableWithType(Type type)
		{
			return false;
		}

		[InjectOnClient]
		private IHudWindowManager _windowManager;

		[InjectOnClient]
		private IInputActiveDeviceChangeNotifier _activeDeviceChangeNotifier;

		[InjectOnClient]
		private IInputTranslation _inputTranslation;

		[InjectOnClient]
		private ISpectatorCameraConfigProvider _cameraConfigProvider;

		private ISpectatorHelperView _view;

		private bool _showing;

		private IDisposable _disposable;

		private bool _isGamepad;
	}
}
