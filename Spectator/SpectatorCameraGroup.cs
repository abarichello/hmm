using System;
using System.Collections.Generic;
using HeavyMetalMachines.Input.ControllerInput;
using Hoplon.Logging;

namespace HeavyMetalMachines.Spectator
{
	public class SpectatorCameraGroup
	{
		public SpectatorCameraGroup(SpectatorCameraGroupType spectatorCameraGroupType, ControllerInputActions inputAction, ILogger<SpectatorCameraGroup> logger)
		{
			this._spectatorCameraGroupType = spectatorCameraGroupType;
			this._inputAction = inputAction;
			this._logger = logger;
			this._availableCameras = new List<ICameraAngle>();
		}

		public SpectatorCameraGroupType SpectatorCameraGroupType
		{
			get
			{
				return this._spectatorCameraGroupType;
			}
		}

		public ControllerInputActions InputAction
		{
			get
			{
				return this._inputAction;
			}
		}

		public void AddCamera(ICameraAngle camera)
		{
			if (!this._availableCameras.Contains(camera))
			{
				this._availableCameras.Add(camera);
			}
		}

		public void RemoveCamera(ICameraAngle camera)
		{
			this._availableCameras.Remove(camera);
		}

		public ICameraAngle GetCamera()
		{
			if (this._availableCameras.Count == 0)
			{
				this._logger.Error("SpectatorCameraGroup: No availableCameras");
				return null;
			}
			this._currentCameraIndex++;
			if (this._currentCameraIndex > this._availableCameras.Count - 1)
			{
				this._currentCameraIndex = 0;
			}
			return this._availableCameras[this._currentCameraIndex];
		}

		private readonly List<ICameraAngle> _availableCameras;

		private readonly SpectatorCameraGroupType _spectatorCameraGroupType;

		private readonly ControllerInputActions _inputAction;

		private ILogger<SpectatorCameraGroup> _logger;

		private int _currentCameraIndex;
	}
}
