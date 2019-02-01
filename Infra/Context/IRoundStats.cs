using System;

namespace HeavyMetalMachines.Infra.Context
{
	public interface IRoundStats
	{
		float BombTimeRed { get; }

		float BombTimeBlue { get; }

		int Deliverer { get; }

		float DeliveryTime { get; }

		int DeliverTeam { get; }
	}
}
