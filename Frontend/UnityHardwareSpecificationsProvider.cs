using System;
using HeavyMetalMachines.HardwareAnalysis;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class UnityHardwareSpecificationsProvider : IHardwareSpecificationsProvider
	{
		public int GetMemorySizeInMegabytes()
		{
			return SystemInfo.systemMemorySize;
		}

		public GpuPerformanceIndex GetVideoCardPerformanceIndex()
		{
			return GPUPerformance.GetVideoCardPerformanceIndex();
		}
	}
}
