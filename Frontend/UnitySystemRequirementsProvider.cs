using System;
using HeavyMetalMachines.HardwareAnalysis;

namespace HeavyMetalMachines.Frontend
{
	public class UnitySystemRequirementsProvider : ISystemRequirementsProvider
	{
		public UnitySystemRequirementsProvider(SystemCheckSettings systemCheckSettings)
		{
			this._systemCheckSettings = systemCheckSettings;
		}

		public SystemRequirements Get()
		{
			return new SystemRequirements
			{
				MinimumMemoryInMegabytes = this._systemCheckSettings.MemoryMinimumRequirement,
				WarningMinimumMemoryInMegabytes = this._systemCheckSettings.MemoryWarningRequirement
			};
		}

		private readonly SystemCheckSettings _systemCheckSettings;
	}
}
