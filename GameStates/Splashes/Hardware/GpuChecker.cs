using System;
using Pocketverse;

namespace HeavyMetalMachines.GameStates.Splashes.Hardware
{
	internal class GpuChecker : IHardwareChecker
	{
		public bool HasMinimumRequirements()
		{
			int videoCardPerformanceIndex = GPUPerformance.GetVideoCardPerformanceIndex();
			bool flag = videoCardPerformanceIndex >= 399;
			NativePlugins.CrashReport.SetGpuStatus((!flag) ? "FAILED" : "PASSED");
			return GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.IgnoreVideoCardCheck) || flag;
		}

		public bool HasWarningRequirements()
		{
			return true;
		}
	}
}
