using System;

namespace HeavyMetalMachines.Combat
{
	public interface IPlayerSpawnListener
	{
		void OnPlayerSpawned(PlayerSpawnEvent evt);
	}
}
