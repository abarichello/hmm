using System;
using HeavyMetalMachines.HardwareAnalysis;
using HeavyMetalMachines.Windows;
using UnityEngine;

namespace HeavyMetalMachines
{
	public static class CrashReporter
	{
		static CrashReporter()
		{
			RuntimePlatform platform = Application.platform;
			switch (platform)
			{
			case 13:
			case 16:
				break;
			default:
				switch (platform)
				{
				case 25:
				case 27:
					break;
				default:
					if (platform == 2 || platform == 7)
					{
						CrashReporter._reporter = new WindowsCrashReport();
						return;
					}
					if (platform != 32)
					{
						throw new Exception("Unsupported platform");
					}
					break;
				}
				break;
			}
			CrashReporter._reporter = new VoidCrashReporter();
		}

		public static ICrashReporter Get()
		{
			return CrashReporter._reporter;
		}

		public static void Initialize()
		{
			CrashReporter._reporter.Initialize();
		}

		public static void SetCurrentScene(string scene)
		{
			CrashReporter._reporter.SetCurrentScene(scene);
		}

		public static void SetRamStatus(string status)
		{
			CrashReporter._reporter.SetRamStatus(status);
		}

		public static void SetGpuStatus(string status)
		{
			CrashReporter._reporter.SetGpuStatus(status);
		}

		public static void SetFirstTime(bool isFirstTime)
		{
			CrashReporter._reporter.SetFirstTime(isFirstTime);
		}

		private static readonly ICrashReporter _reporter;
	}
}
