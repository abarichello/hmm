using System;

namespace HeavyMetalMachines.Platforms
{
	public class IsOnConsolePlatform : IIsOnConsolePlatform
	{
		public bool Check()
		{
			return Platform.Current.IsConsole();
		}
	}
}
