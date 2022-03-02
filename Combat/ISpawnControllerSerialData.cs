using System;
using HeavyMetalMachines.Playback.Snapshot;

namespace HeavyMetalMachines.Combat
{
	public interface ISpawnControllerSerialData : IBaseStreamSerialData<ISpawnControllerSerialData>
	{
		SpawnStateKind State { get; }

		int SpawnTime { get; }

		int UnspawnTime { get; }
	}
}
