using System;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input.ControllerInput;

namespace HeavyMetalMachines.Spectator
{
	public class SpectatorCameraConfigProvider : ISpectatorCameraConfigProvider
	{
		public ControllerInputActions GetFixedCamerasInputAction(SpectatorCameraGroupType cameraGroupType)
		{
			for (int i = 0; i < this._spectatorCameraConfig.FixedCamerasGroupConfig.Count; i++)
			{
				SpectatorCameraGroupInputMapper spectatorCameraGroupInputMapper = this._spectatorCameraConfig.FixedCamerasGroupConfig[i];
				if (spectatorCameraGroupInputMapper.CameraGroupType == cameraGroupType)
				{
					return spectatorCameraGroupInputMapper.Action;
				}
			}
			return -1;
		}

		public ControllerInputActions ToggleFocusOnBombGamepadAction
		{
			get
			{
				return this._spectatorCameraConfig.ToggleFocusOnBombGamepadAction;
			}
		}

		public ControllerInputActions ToggleOrbitalCameraGamepadAction
		{
			get
			{
				return this._spectatorCameraConfig.ToggleOrbitalCameraGamepadAction;
			}
		}

		public ControllerInputActions ToggleFreeCameraGamepadAction
		{
			get
			{
				return this._spectatorCameraConfig.ToggleFreeCameraGamepadAction;
			}
		}

		public ControllerInputActions ZoomInGamepadAction
		{
			get
			{
				return this._spectatorCameraConfig.ZoomInGamepadAction;
			}
		}

		public ControllerInputActions ZoomOutGamepadAction
		{
			get
			{
				return this._spectatorCameraConfig.ZoomOutGamepadAction;
			}
		}

		public ControllerInputActions MoveFreeCameraHorizontalGamepadAxisAction
		{
			get
			{
				return this._spectatorCameraConfig.MoveFreeCameraHorizontalGamepadAxisAction;
			}
		}

		public ControllerInputActions MoveFreeCameraVerticallGamepadAxisAction
		{
			get
			{
				return this._spectatorCameraConfig.MoveFreeCameraVerticallGamepadAxisAction;
			}
		}

		public ControllerInputActions NextCharacterFocusGamepadAction
		{
			get
			{
				return this._spectatorCameraConfig.NextCharacterFocusGamepadAction;
			}
		}

		public ControllerInputActions PreviousCharacterFocusGamepadAction
		{
			get
			{
				return this._spectatorCameraConfig.PreviousCharacterFocusGamepadAction;
			}
		}

		public ControllerInputActions MoveOrbitalCameraHorizontalGamepadAxisAction
		{
			get
			{
				return this._spectatorCameraConfig.MoveOrbitalCameraHorizontalGamepadAxisAction;
			}
		}

		public ControllerInputActions MoveOrbitalCameraVerticallGamepadAxisAction
		{
			get
			{
				return this._spectatorCameraConfig.MoveOrbitalCameraVerticallGamepadAxisAction;
			}
		}

		[InjectOnClient]
		private SpectatorCameraConfig _spectatorCameraConfig;
	}
}
