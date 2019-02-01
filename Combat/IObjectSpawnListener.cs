using System;

namespace HeavyMetalMachines.Combat
{
	public interface IObjectSpawnListener
	{
		void OnObjectUnspawned(UnspawnEvent evt);

		void OnObjectSpawned(SpawnEvent evt);
	}
}
