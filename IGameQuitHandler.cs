using System;

namespace HeavyMetalMachines
{
	public interface IGameQuitHandler : IDisposable
	{
		void Quit(GameQuitReason quitReason, string detail = "");
	}
}
