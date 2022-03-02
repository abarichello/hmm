using System;
using HeavyMetalMachines.Input.ControllerInput;
using UnityEngine;

namespace HeavyMetalMachines.Options
{
	[Serializable]
	public struct OptionWindowControllerTabInputCategory
	{
		[Header("[Title Name - Options Sheet]")]
		public string TitleDraft;

		public ControllerInputActions[] ControllerInputActions;
	}
}
