using System;
using System.Collections.Generic;
using HeavyMetalMachines.Input.ControllerInput;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.InGame
{
	[Serializable]
	public class InputModifierInfo : MonoBehaviour
	{
		public bool lockAllInputs;

		public bool unlockAllInputs;

		public List<ControllerInputActions> UnlockedPlayerInputActions;
	}
}
