using System;
using HeavyMetalMachines.Durango;
using HeavyMetalMachines.Linux;
using HeavyMetalMachines.Orbis;
using HeavyMetalMachines.Windows;
using UnityEngine;

namespace HeavyMetalMachines
{
	public static class Platform
	{
		static Platform()
		{
			RuntimePlatform platform = Application.platform;
			switch (platform)
			{
			case 13:
			case 16:
				Platform.Current = new LinuxPlatform();
				break;
			default:
				switch (platform)
				{
				case 25:
					Platform.Current = new OrbisPlatform();
					break;
				default:
					if (platform != 2 && platform != 7)
					{
						if (platform != 32)
						{
						}
						throw new Exception("Unsupported platform");
					}
					Platform.Current = new WindowsPlatform();
					break;
				case 27:
					Platform.Current = new DurangoPlatform();
					break;
				}
				break;
			}
		}

		public static readonly IPlatform Current;
	}
}
