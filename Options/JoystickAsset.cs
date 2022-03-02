using System;
using Hoplon.Input;

namespace HeavyMetalMachines.Options
{
	[Serializable]
	public struct JoystickAsset
	{
		public string Alias;

		public JoystickHardware JoystickHardware;

		public JoystickIconAsset[] JoystickIconAssets;
	}
}
