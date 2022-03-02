using System;

namespace HeavyMetalMachines
{
	public interface ICollisionDispatcher
	{
		void SendData(CollisionEvent evt);
	}
}
