using System;
using HeavyMetalMachines.Input.ControllerInput;

namespace HeavyMetalMachines.Spectator
{
	public interface ISpectatorCameraConfigProvider
	{
		ControllerInputActions GetFixedCamerasInputAction(SpectatorCameraGroupType cameraGroupType);

		ControllerInputActions ToggleFocusOnBombGamepadAction { get; }

		ControllerInputActions ToggleOrbitalCameraGamepadAction { get; }

		ControllerInputActions ToggleFreeCameraGamepadAction { get; }

		ControllerInputActions ZoomInGamepadAction { get; }

		ControllerInputActions ZoomOutGamepadAction { get; }

		ControllerInputActions MoveFreeCameraHorizontalGamepadAxisAction { get; }

		ControllerInputActions MoveFreeCameraVerticallGamepadAxisAction { get; }

		ControllerInputActions NextCharacterFocusGamepadAction { get; }

		ControllerInputActions PreviousCharacterFocusGamepadAction { get; }

		ControllerInputActions MoveOrbitalCameraHorizontalGamepadAxisAction { get; }

		ControllerInputActions MoveOrbitalCameraVerticallGamepadAxisAction { get; }
	}
}
