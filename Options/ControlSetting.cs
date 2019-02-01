using System;
using HeavyMetalMachines.Car;
using Pocketverse;

namespace HeavyMetalMachines.Options
{
	public class ControlSetting : GameHubScriptableObject
	{
		public CarInput.DrivingStyleKind DefaultMovementMode;

		public bool InvertReverseControl;

		public string[] Modifiers;

		public string[] ForbiddenKeys;

		public bool AllowDuplicates;

		public const string KeyboadLayoutDefault = "QWERTY";
	}
}
