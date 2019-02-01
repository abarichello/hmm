using System;

namespace HeavyMetalMachines.Infra.Context
{
	public interface IArena
	{
		float BombPosition { get; }

		IPhysicalObject DeliveryRed { get; }

		IPhysicalObject DeliveryBlue { get; }
	}
}
