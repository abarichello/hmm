using System;
using System.Runtime.InteropServices;

namespace HeavyMetalMachines.Windows
{
	internal static class PsApi
	{
		[DllImport("psapi.dll")]
		public static extern bool GetProcessMemoryInfo(IntPtr process, out PsApi.ProcessMemoryCountersEx counters, uint cb);

		public struct ProcessMemoryCountersEx
		{
			public uint Cb;

			public uint PageFaultCount;

			public IntPtr PeakWorkingSetSize;

			public IntPtr WorkingSetSize;

			public IntPtr QuotaPeakPagedPoolUsage;

			public IntPtr QuotaPagedPoolUsage;

			public IntPtr QuotaPeakNonPagedPoolUsage;

			public IntPtr QuotaNonPagedPoolUsage;

			public IntPtr PagefileUsage;

			public IntPtr PeakPagefileUsage;

			public IntPtr PrivateUsage;
		}
	}
}
