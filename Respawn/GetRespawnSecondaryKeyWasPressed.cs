using System;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Options;

namespace HeavyMetalMachines.Respawn
{
	public class GetRespawnSecondaryKeyWasPressed : IGetRespawnSecondaryKeyWasPressed
	{
		public GetRespawnSecondaryKeyWasPressed(IControlSetting controlSetting, IControllerInputActionPoller inputActionPoller)
		{
			this._controlSetting = controlSetting;
			this._inputActionPoller = inputActionPoller;
		}

		public bool WasPressed()
		{
			foreach (ControllerInputActions controllerInputActions in this._controlSetting.SecondaryRespawnJoystickKeys)
			{
				bool buttonDown = this._inputActionPoller.GetButtonDown(controllerInputActions);
				if (buttonDown)
				{
					return true;
				}
			}
			return false;
		}

		private readonly IControlSetting _controlSetting;

		private readonly IControllerInputActionPoller _inputActionPoller;
	}
}
