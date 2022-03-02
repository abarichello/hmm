using System;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Presenting;
using Hoplon.Input;
using Hoplon.Input.Business;

namespace HeavyMetalMachines.Options
{
	public interface IControlSetting
	{
		float UiNavigationAxisThreshold { get; }

		float BindWindowStartPreBindTimeoutInSec { get; }

		float BindWindowWaitForConflictAnswerTimeoutInSec { get; }

		float BindWindowShowCompletedTimeoutInSec { get; }

		ControllerInputActions[] SecondaryRespawnJoystickKeys { get; }

		bool IsKeyForbidden(KeyboardMouseCode keyboardMouseCode);

		bool IsJoystickTemplateCodeForbidden(JoystickTemplateCode joystickTemplateCode);

		bool TryToGetMouseSprite(KeyboardMouseCode keyboardMouseCode, out ISprite iconSprite);

		bool TryToGetJoystickKeyIconSprite(JoystickTemplateCode joystickTemplateCode, JoystickHardware joystickHardware, out ISprite joystickKeyIconSprite);

		string GetJoystickLocalizationKey(JoystickTemplateCode joystickTemplateCode, JoystickHardware joystickHardware);

		bool TryToGetCameraPanCodeDefaultNormalizedValue(CameraPanCode cameraPanCode, out float defaultNormalizedValue);

		bool TryToGetCameraPanCodeInfraValues(CameraPanCode cameraPanCode, out float minInfraValue, out float maxInfraValue);

		bool ActionIsInOptions(int actionId);

		ControllerInputActionName[] GetControllerInputActionNames();

		OptionWindowControllerTab GetOptionWindowControllerTab();
	}
}
