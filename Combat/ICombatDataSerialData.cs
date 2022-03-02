using System;
using HeavyMetalMachines.Playback.Snapshot;

namespace HeavyMetalMachines.Combat
{
	public interface ICombatDataSerialData : IBaseStreamSerialData<ICombatDataSerialData>
	{
		float HP { get; }

		float HPTemp { get; }

		float EP { get; }
	}
}
