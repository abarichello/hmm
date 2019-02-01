using System;

namespace HeavyMetalMachines.GameStates.Splashes.Hardware
{
	internal interface IHardwareChecker
	{
		bool HasMinimumRequirements();

		bool HasWarningRequirements();
	}
}
