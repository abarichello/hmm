using System;
using System.Collections.Generic;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input.ControllerInput;
using Hoplon.Logging;
using Zenject;

namespace HeavyMetalMachines.Spectator
{
	public class SpectatorCameraManager : ISpectatorCameraManager
	{
		public void RegisterCameraAngle(ICameraAngle cameraAngle)
		{
			if (!this._availableAngles.Contains(cameraAngle))
			{
				this._availableAngles.Add(cameraAngle);
			}
			SpectatorCameraGroup spectatorCameraGroup;
			if (this.CameraGroupContainsGroup(cameraAngle.SpectatorCameraGroupType, out spectatorCameraGroup))
			{
				spectatorCameraGroup.AddCamera(cameraAngle);
			}
			else
			{
				this.CreateAndAddCameraGroup(cameraAngle);
			}
		}

		public void UnregisterCameraAngle(ICameraAngle cameraAngle)
		{
			this._availableAngles.Remove(cameraAngle);
			SpectatorCameraGroup spectatorCameraGroup;
			if (this.CameraGroupContainsGroup(cameraAngle.SpectatorCameraGroupType, out spectatorCameraGroup))
			{
				spectatorCameraGroup.RemoveCamera(cameraAngle);
			}
		}

		public void UpdateCamera()
		{
			for (int i = 0; i < this._availableAngles.Count; i++)
			{
				ICameraAngle cameraAngle = this._availableAngles[i];
				if (cameraAngle.HotKey.IsPressed())
				{
					this._spectator.SetFixedCamera(cameraAngle);
				}
			}
			for (int j = 0; j < this._toggleableCamerasGroup.Count; j++)
			{
				SpectatorCameraGroup spectatorCameraGroup = this._toggleableCamerasGroup[j];
				if (this._controllerInputActionPoller.GetButtonDown(spectatorCameraGroup.InputAction))
				{
					ICameraAngle camera = spectatorCameraGroup.GetCamera();
					if (camera != null)
					{
						this._spectator.SetFixedCamera(camera);
					}
				}
			}
		}

		private void CreateAndAddCameraGroup(ICameraAngle cameraAngle)
		{
			ControllerInputActions fixedCamerasInputAction = this._cameraConfigProvider.GetFixedCamerasInputAction(cameraAngle.SpectatorCameraGroupType);
			ILogger<SpectatorCameraGroup> logger = this._diContainer.Resolve<ILogger<SpectatorCameraGroup>>();
			SpectatorCameraGroup spectatorCameraGroup = new SpectatorCameraGroup(cameraAngle.SpectatorCameraGroupType, fixedCamerasInputAction, logger);
			spectatorCameraGroup.AddCamera(cameraAngle);
			this._toggleableCamerasGroup.Add(spectatorCameraGroup);
		}

		private bool CameraGroupContainsGroup(SpectatorCameraGroupType groupType, out SpectatorCameraGroup cameraGroup)
		{
			for (int i = 0; i < this._toggleableCamerasGroup.Count; i++)
			{
				SpectatorCameraGroup spectatorCameraGroup = this._toggleableCamerasGroup[i];
				if (spectatorCameraGroup.SpectatorCameraGroupType == groupType)
				{
					cameraGroup = spectatorCameraGroup;
					return true;
				}
			}
			cameraGroup = null;
			return false;
		}

		[InjectOnClient]
		private ISpectatorService _spectator;

		[InjectOnClient]
		private IControllerInputActionPoller _controllerInputActionPoller;

		[InjectOnClient]
		private ISpectatorCameraConfigProvider _cameraConfigProvider;

		[InjectOnClient]
		private DiContainer _diContainer;

		private readonly List<ICameraAngle> _availableAngles = new List<ICameraAngle>();

		private readonly List<SpectatorCameraGroup> _toggleableCamerasGroup = new List<SpectatorCameraGroup>();
	}
}
