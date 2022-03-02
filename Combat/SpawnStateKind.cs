using System;

namespace HeavyMetalMachines.Combat
{
	public enum SpawnStateKind : byte
	{
		None,
		Spawned,
		Unspawned,
		Pooled,
		PreSpawned,
		Respawning
	}
}
