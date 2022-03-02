using System;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Localization;

namespace HeavyMetalMachines.Tutorial
{
	[Serializable]
	public class TutorialDataDescription
	{
		public string Draft;

		public TranslationSheets Sheet;

		public ControllerInputActions ControllerInputAction;
	}
}
