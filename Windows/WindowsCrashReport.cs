using System;
using System.Runtime.InteropServices;
using HeavyMetalMachines.HardwareAnalysis;
using UnityEngine;

namespace HeavyMetalMachines.Windows
{
	internal class WindowsCrashReport : ICrashReporter
	{
		[DllImport("crashhandler.plugin")]
		private static extern void InitCrashHandler();

		[DllImport("crashhandler.plugin", CharSet = CharSet.Ansi)]
		private static extern void SetCrashCurrentScene(string scene);

		[DllImport("crashhandler.plugin", CharSet = CharSet.Ansi)]
		private static extern void SetCrashRamStatus(string status);

		[DllImport("crashhandler.plugin", CharSet = CharSet.Ansi)]
		private static extern void SetCrashGpuStatus(string status);

		[DllImport("crashhandler.plugin")]
		private static extern void SetCrashFirstTime(bool isFirstTime);

		public void Initialize()
		{
			if (WindowsCrashReport._isInitialized)
			{
				Debug.LogError("CrashReport already initialized!");
				return;
			}
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			if (!Array.Exists<string>(commandLineArgs, (string arg) => arg == "--crashHandler=BugSplat"))
			{
				if (Array.Exists<string>(commandLineArgs, (string arg) => arg == "--crashHandler=Legacy"))
				{
					WindowsCrashReport._reporter = WindowsCrashReport.Reporter.Legacy;
				}
			}
			WindowsCrashReport.Reporter reporter = WindowsCrashReport._reporter;
			if (reporter != WindowsCrashReport.Reporter.Legacy)
			{
				if (reporter != WindowsCrashReport.Reporter.None)
				{
				}
			}
			else
			{
				WindowsCrashReport.InitCrashHandler();
			}
			WindowsCrashReport._isInitialized = true;
		}

		public void SetCurrentScene(string scene)
		{
			if (!WindowsCrashReport._isInitialized)
			{
				Debug.LogWarning("CrashReport not initialized!");
				this.Initialize();
			}
			WindowsCrashReport.Reporter reporter = WindowsCrashReport._reporter;
			if (reporter != WindowsCrashReport.Reporter.Legacy)
			{
				if (reporter != WindowsCrashReport.Reporter.None)
				{
				}
			}
			else
			{
				WindowsCrashReport.SetCrashCurrentScene(scene);
			}
		}

		public void SetRamStatus(string status)
		{
			if (!WindowsCrashReport._isInitialized)
			{
				Debug.LogWarning("CrashReport not initialized!");
				this.Initialize();
			}
			WindowsCrashReport.Reporter reporter = WindowsCrashReport._reporter;
			if (reporter != WindowsCrashReport.Reporter.Legacy)
			{
				if (reporter != WindowsCrashReport.Reporter.None)
				{
				}
			}
			else
			{
				WindowsCrashReport.SetCrashRamStatus(status);
			}
		}

		public void SetGpuStatus(string status)
		{
			if (!WindowsCrashReport._isInitialized)
			{
				Debug.LogWarning("CrashReport not initialized!");
				this.Initialize();
			}
			WindowsCrashReport.Reporter reporter = WindowsCrashReport._reporter;
			if (reporter != WindowsCrashReport.Reporter.Legacy)
			{
				if (reporter != WindowsCrashReport.Reporter.None)
				{
				}
			}
			else
			{
				WindowsCrashReport.SetCrashGpuStatus(status);
			}
		}

		public void SetFirstTime(bool isFirstTime)
		{
			if (!WindowsCrashReport._isInitialized)
			{
				Debug.LogWarning("CrashReport not initialized!");
				this.Initialize();
			}
			WindowsCrashReport.Reporter reporter = WindowsCrashReport._reporter;
			if (reporter != WindowsCrashReport.Reporter.Legacy)
			{
				if (reporter != WindowsCrashReport.Reporter.None)
				{
				}
			}
			else
			{
				WindowsCrashReport.SetCrashFirstTime(isFirstTime);
			}
		}

		private static WindowsCrashReport.Reporter _reporter;

		private static bool _isInitialized;

		private enum Reporter
		{
			None,
			Legacy
		}
	}
}
