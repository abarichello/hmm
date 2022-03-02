using System;
using Hoplon.SensorSystem;

namespace HeavyMetalMachines.SensorSystem
{
	public interface ISensorContextProvider
	{
		ISensorController SensorContext { get; }
	}
}
