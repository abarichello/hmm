using System;
using Pocketverse;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class FakeLogOutput : ILogOutput
	{
		public void Log(LogType type, string tag, object e)
		{
		}
	}
}
