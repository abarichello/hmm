using System;
using System.Collections.Generic;
using HeavyMetalMachines.Options;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.InGame
{
	[Serializable]
	public class InputModifierInfo : MonoBehaviour
	{
		public bool lockAllInputs;

		public bool unlockAllInputs;

		public List<ControlAction> UnlockedPlayerControls;
	}
}
