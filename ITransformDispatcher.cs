using System;
using System.Collections.Generic;
using HeavyMetalMachines.UpdateStream;

namespace HeavyMetalMachines
{
	public interface ITransformDispatcher
	{
		void SendMovementData(List<MovementStream> movementStreams);
	}
}
