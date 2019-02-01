using System;
using HeavyMetalMachines.Frontend;

namespace HeavyMetalMachines.GameStates.Splashes.Hardware
{
	internal class HardwareChecker : IHardwareChecker
	{
		public HardwareChecker(SystemCheckSettings systemCheckScriptableObject)
		{
			this._memoryChecker = new MemoryChecker(systemCheckScriptableObject);
			this._gpuChecker = new GpuChecker();
		}

		public bool HasMinimumRequirements()
		{
			return this._memoryChecker.HasMinimumRequirements() && this._gpuChecker.HasMinimumRequirements();
		}

		public bool HasWarningRequirements()
		{
			return this._memoryChecker.HasWarningRequirements() && this._gpuChecker.HasWarningRequirements();
		}

		private readonly MemoryChecker _memoryChecker;

		private readonly GpuChecker _gpuChecker;
	}
}
