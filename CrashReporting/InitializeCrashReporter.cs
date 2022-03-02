using System;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.HardwareAnalysis;

namespace HeavyMetalMachines.CrashReporting
{
	public class InitializeCrashReporter : IInitializeCrashReporter
	{
		public InitializeCrashReporter(UserInfo userInfo)
		{
			this._userInfo = userInfo;
		}

		public void Initialize()
		{
			PlayerBag bag = this._userInfo.Bag;
			CrashReporter.SetFirstTime(!bag.HasDoneTutorial);
		}

		private readonly UserInfo _userInfo;
	}
}
