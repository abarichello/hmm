using System;

namespace HeavyMetalMachines
{
	internal class VoidGameQuitHandler : IGameQuitHandler, IDisposable
	{
		public void Dispose()
		{
		}

		public void Quit(GameQuitReason quitReason, string detail = "")
		{
		}
	}
}
