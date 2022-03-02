using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace HeavyMetalMachines.Windows
{
	internal static class Kernel32
	{
		public static void TerminateCurrentProcess()
		{
			IntPtr currentProcess = Kernel32.GetCurrentProcess();
			if (!Kernel32.TerminateProcess(currentProcess, 0U))
			{
				return;
			}
			Kernel32.WaitForSingleObject(currentProcess, uint.MaxValue);
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool GetSystemTimes(out System.Runtime.InteropServices.ComTypes.FILETIME lpIdleTime, out System.Runtime.InteropServices.ComTypes.FILETIME lpKernelTime, out System.Runtime.InteropServices.ComTypes.FILETIME lpUserTime);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GlobalMemoryStatusEx([In] [Out] Kernel32.MemoryStatusEx lpBuffer);

		[DllImport("kernel32.dll")]
		public static extern void RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, IntPtr lpArguments);

		[DllImport("kernel32.dll")]
		public static extern uint GetCurrentThreadId();

		[DllImport("kernel32")]
		public static extern IntPtr GetCurrentProcess();

		[DllImport("kernel32")]
		public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

		[DllImport("kernel32")]
		public static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

		[DllImport("kernel32.dll")]
		public static extern int GetSystemDefaultLCID();

		public const uint Infinite = 4294967295U;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public class MemoryStatusEx
		{
			public MemoryStatusEx()
			{
				this.Length = (uint)Marshal.SizeOf(typeof(Kernel32.MemoryStatusEx));
			}

			public uint Length;

			public uint MemoryLoad;

			public ulong TotalPhysicalMemory;

			public ulong AvailablePhysicalMemory;

			public ulong TotalPageFile;

			public ulong AvailPageFile;

			public ulong TotalVirtualMemory;

			public ulong AvailableVirtualMemory;

			public ulong AvailableExtendedVirtualMemory;
		}
	}
}
