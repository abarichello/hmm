using System;
using HeavyMetalMachines.Frontend;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.GameStates.Splashes.Hardware
{
	public class MemoryChecker : IHardwareChecker
	{
		public MemoryChecker(SystemCheckSettings systemCheckScriptableObject)
		{
			this._systemCheckScriptableObject = systemCheckScriptableObject;
		}

		public bool HasMinimumRequirements()
		{
			int systemMemorySize = SystemInfo.systemMemorySize;
			bool flag = systemMemorySize > this._systemCheckScriptableObject.MemoryMinimumRequirement;
			NativePlugins.CrashReport.SetRamStatus((!flag) ? "FAILED" : "PASSED");
			return GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.IgnoreMemoryCheck) || flag;
		}

		public bool HasWarningRequirements()
		{
			return SystemInfo.systemMemorySize > this._systemCheckScriptableObject.MemoryWarningRequirement;
		}

		private readonly SystemCheckSettings _systemCheckScriptableObject;
	}
}
