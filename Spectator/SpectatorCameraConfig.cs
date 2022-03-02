using System;
using System.Collections.Generic;
using HeavyMetalMachines.Input.ControllerInput;
using UnityEngine;

namespace HeavyMetalMachines.Spectator
{
	public class SpectatorCameraConfig : ScriptableObject
	{
		public List<SpectatorCameraGroupInputMapper> FixedCamerasGroupConfig
		{
			get
			{
				return this._fixedCamerasGroupConfig;
			}
		}

		public ControllerInputActions ToggleFocusOnBombGamepadAction
		{
			get
			{
				return this._toggleFocusOnBombGamepadAction;
			}
		}

		public ControllerInputActions ToggleOrbitalCameraGamepadAction
		{
			get
			{
				return this._toggleOrbitalCameraGamepadAction;
			}
		}

		public ControllerInputActions ToggleFreeCameraGamepadAction
		{
			get
			{
				return this._toggleFreeCameraGamepadAction;
			}
		}

		public ControllerInputActions ZoomInGamepadAction
		{
			get
			{
				return this._zoomInGamepadAction;
			}
		}

		public ControllerInputActions ZoomOutGamepadAction
		{
			get
			{
				return this._zoomOutGamepadAction;
			}
		}

		public ControllerInputActions MoveFreeCameraHorizontalGamepadAxisAction
		{
			get
			{
				return this._moveFreeCameraHorizontalGamepadAxisAction;
			}
		}

		public ControllerInputActions MoveFreeCameraVerticallGamepadAxisAction
		{
			get
			{
				return this._moveFreeCameraVerticallGamepadAxisAction;
			}
		}

		public ControllerInputActions MoveOrbitalCameraHorizontalGamepadAxisAction
		{
			get
			{
				return this._moveOrbitalCameraHorizontalGamepadAxisAction;
			}
		}

		public ControllerInputActions MoveOrbitalCameraVerticallGamepadAxisAction
		{
			get
			{
				return this._moveOrbitalCameraVerticallGamepadAxisAction;
			}
		}

		public ControllerInputActions NextCharacterFocusGamepadAction
		{
			get
			{
				return this._nextCharacterFocusGamepadAction;
			}
		}

		public ControllerInputActions PreviousCharacterFocusGamepadAction
		{
			get
			{
				return this._previousCharacterFocusGamepadAction;
			}
		}

		[SerializeField]
		private List<SpectatorCameraGroupInputMapper> _fixedCamerasGroupConfig;

		[SerializeField]
		private ControllerInputActions _toggleFocusOnBombGamepadAction;

		[SerializeField]
		private ControllerInputActions _toggleOrbitalCameraGamepadAction;

		[SerializeField]
		private ControllerInputActions _toggleFreeCameraGamepadAction;

		[SerializeField]
		private ControllerInputActions _zoomInGamepadAction;

		[SerializeField]
		private ControllerInputActions _zoomOutGamepadAction;

		[SerializeField]
		private ControllerInputActions _moveFreeCameraHorizontalGamepadAxisAction;

		[SerializeField]
		private ControllerInputActions _moveFreeCameraVerticallGamepadAxisAction;

		[SerializeField]
		private ControllerInputActions _moveOrbitalCameraHorizontalGamepadAxisAction;

		[SerializeField]
		private ControllerInputActions _moveOrbitalCameraVerticallGamepadAxisAction;

		[SerializeField]
		private ControllerInputActions _nextCharacterFocusGamepadAction;

		[SerializeField]
		private ControllerInputActions _previousCharacterFocusGamepadAction;
	}
}
