using System;

namespace HeavyMetalMachines.Combat
{
	public interface IBombInstanceDispatcher
	{
		void Update(int causerId, BombInstance bomb, SpawnReason reason);

		void UpdateDataTo(byte playerAddress, BombInstance bomb);
	}
}
