using System;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input;
using Hoplon.Input.Business;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Options
{
	public class ControlSetting : GameHubScriptableObject, IControlSetting
	{
		public float UiNavigationAxisThreshold
		{
			get
			{
				return this._uiNavigationAxisThreshold;
			}
		}

		public float BindWindowStartPreBindTimeoutInSec
		{
			get
			{
				return this._bindWindowStartPreBindTimeoutInSec;
			}
		}

		public float BindWindowWaitForConflictAnswerTimeoutInSec
		{
			get
			{
				return this._bindWindowWaitForConflictAnswerTimeoutInSec;
			}
		}

		public float BindWindowShowCompletedTimeoutInSec
		{
			get
			{
				return this._bindWindowShowCompletedTimeoutInSec;
			}
		}

		public ControllerInputActions[] SecondaryRespawnJoystickKeys
		{
			get
			{
				return this._secondaryRespawnJoystickKeys;
			}
		}

		public bool IsKeyForbidden(KeyboardMouseCode keyboardMouseCode)
		{
			KeyboardMouseCode[] forbiddenKeys = this.ForbiddenKeys;
			for (int i = 0; i < forbiddenKeys.Length; i++)
			{
				if (keyboardMouseCode == forbiddenKeys[i])
				{
					return true;
				}
			}
			return false;
		}

		public bool IsJoystickTemplateCodeForbidden(JoystickTemplateCode joystickTemplateCode)
		{
			JoystickTemplateCode[] forbiddenJoystickKeys = this.ForbiddenJoystickKeys;
			for (int i = 0; i < forbiddenJoystickKeys.Length; i++)
			{
				if (joystickTemplateCode == forbiddenJoystickKeys[i])
				{
					return true;
				}
			}
			return false;
		}

		public bool TryToGetMouseSprite(KeyboardMouseCode keyboardMouseCode, out ISprite iconSprite)
		{
			foreach (MouseIconAsset mouseIconAsset in this.MouseIconAssets)
			{
				if (mouseIconAsset.MouseCode == keyboardMouseCode)
				{
					iconSprite = new UnitySprite(mouseIconAsset.IconSprite);
					return true;
				}
			}
			iconSprite = null;
			return false;
		}

		public bool TryToGetJoystickKeyIconSprite(JoystickTemplateCode joystickTemplateCode, JoystickHardware joystickHardware, out ISprite joystickKeyIconSprite)
		{
			foreach (JoystickAsset joystickAsset in this.JoystickAssets)
			{
				if (joystickAsset.JoystickHardware == joystickHardware)
				{
					return this.TryToGetJoystickKeyIconSprite(joystickTemplateCode, joystickAsset, out joystickKeyIconSprite);
				}
			}
			joystickKeyIconSprite = null;
			return false;
		}

		public string GetJoystickLocalizationKey(JoystickTemplateCode joystickTemplateCode, JoystickHardware joystickHardware)
		{
			if (joystickTemplateCode == null)
			{
				return "Failed: joystickTemplateCode none";
			}
			foreach (JoystickAsset joystickAsset in this.JoystickAssets)
			{
				if (joystickAsset.JoystickHardware == joystickHardware)
				{
					return this.GetJoystickLocalizationKey(joystickTemplateCode, joystickAsset);
				}
			}
			return string.Format("Failed: JoystickHardware {0} not found", joystickHardware);
		}

		private string GetJoystickLocalizationKey(JoystickTemplateCode joystickTemplateCode, JoystickAsset joystickAsset)
		{
			foreach (JoystickIconAsset joystickIconAsset in joystickAsset.JoystickIconAssets)
			{
				if (joystickIconAsset.JoystickTemplateCode == joystickTemplateCode)
				{
					return joystickIconAsset.Draft;
				}
			}
			return string.Format("Failed: joystickTemplateCode {0} not found on {1}", joystickTemplateCode, joystickAsset.JoystickHardware);
		}

		private bool TryToGetJoystickKeyIconSprite(JoystickTemplateCode joystickTemplateCode, JoystickAsset joystickAsset, out ISprite joystickKeyIconSprite)
		{
			foreach (JoystickIconAsset joystickIconAsset in joystickAsset.JoystickIconAssets)
			{
				if (joystickIconAsset.JoystickTemplateCode == joystickTemplateCode)
				{
					joystickKeyIconSprite = new UnitySprite(joystickIconAsset.IconSprite);
					return true;
				}
			}
			joystickKeyIconSprite = null;
			return false;
		}

		public bool TryToGetCameraPanCodeDefaultNormalizedValue(CameraPanCode cameraPanCode, out float defaultNormalizedValue)
		{
			OptionWindowControllerTabCameraPanItem optionWindowControllerTabCameraPanItem;
			if (this.TryToGetCameraPanItem(cameraPanCode, out optionWindowControllerTabCameraPanItem))
			{
				defaultNormalizedValue = optionWindowControllerTabCameraPanItem.DefaultValuePercent;
				return true;
			}
			defaultNormalizedValue = 0f;
			return false;
		}

		public bool TryToGetCameraPanCodeInfraValues(CameraPanCode cameraPanCode, out float minInfraValue, out float maxInfraValue)
		{
			OptionWindowControllerTabCameraPanItem optionWindowControllerTabCameraPanItem;
			if (this.TryToGetCameraPanItem(cameraPanCode, out optionWindowControllerTabCameraPanItem))
			{
				minInfraValue = optionWindowControllerTabCameraPanItem.InfraMinValue;
				maxInfraValue = optionWindowControllerTabCameraPanItem.InfraMaxValue;
				return true;
			}
			minInfraValue = 0f;
			maxInfraValue = 1f;
			return false;
		}

		private bool TryToGetCameraPanItem(CameraPanCode cameraPanCode, out OptionWindowControllerTabCameraPanItem selectedCameraPanItem)
		{
			OptionWindowControllerTabCameraPanCategory tabCameraPanCategory = this.OptionWindowControllerTab.TabCameraPanCategory;
			foreach (OptionWindowControllerTabCameraPanItem optionWindowControllerTabCameraPanItem in tabCameraPanCategory.CameraPanItems)
			{
				if (cameraPanCode == optionWindowControllerTabCameraPanItem.CameraPanCode)
				{
					selectedCameraPanItem = optionWindowControllerTabCameraPanItem;
					return true;
				}
			}
			selectedCameraPanItem = default(OptionWindowControllerTabCameraPanItem);
			return false;
		}

		public bool ActionIsInOptions(int actionId)
		{
			OptionWindowControllerTabInputCategory[] tabInputCategories = this.OptionWindowControllerTab.TabInputCategories;
			for (int i = 0; i < tabInputCategories.Length; i++)
			{
				ControllerInputActions[] controllerInputActions = tabInputCategories[i].ControllerInputActions;
				for (int j = 0; j < controllerInputActions.Length; j++)
				{
					if (actionId == controllerInputActions[j])
					{
						return true;
					}
				}
			}
			return false;
		}

		public ControllerInputActionName[] GetControllerInputActionNames()
		{
			return this.ControllerInputActionNames;
		}

		public OptionWindowControllerTab GetOptionWindowControllerTab()
		{
			return this.OptionWindowControllerTab;
		}

		[Header("[UiNavigation]")]
		[SerializeField]
		[Range(0.1f, 1f)]
		private float _uiNavigationAxisThreshold = 0.75f;

		[Header("[Bind Window]")]
		[SerializeField]
		private float _bindWindowStartPreBindTimeoutInSec = 5f;

		[SerializeField]
		private float _bindWindowWaitForConflictAnswerTimeoutInSec = 5f;

		[SerializeField]
		private float _bindWindowShowCompletedTimeoutInSec = 1f;

		[Header("[Forbidden Keys]")]
		[SerializeField]
		private KeyboardMouseCode[] ForbiddenKeys;

		[SerializeField]
		private JoystickTemplateCode[] ForbiddenJoystickKeys;

		[Header("[Options Window Layout]")]
		[SerializeField]
		private OptionWindowControllerTab OptionWindowControllerTab;

		[Header("[Action Names - Options Sheet]")]
		[SerializeField]
		private ControllerInputActionName[] ControllerInputActionNames;

		[Header("[Assets]")]
		[SerializeField]
		private MouseIconAsset[] MouseIconAssets;

		[SerializeField]
		private JoystickAsset[] JoystickAssets;

		[Header("[Secondary Joystick Respawn Keys]")]
		[SerializeField]
		private ControllerInputActions[] _secondaryRespawnJoystickKeys;
	}
}
