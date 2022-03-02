using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.UpdateStream
{
	public interface IUpdateManager
	{
		IList<MovementStream> AllStreams { get; }
	}
}
