using System;
using HeavyMetalMachines.Playback.Snapshot;

namespace HeavyMetalMachines.Combat
{
	public interface IGadgetDataSerialData : IBaseStreamSerialData<IGadgetDataSerialData>
	{
		IGadgetStateObjectSerialData BombExplosionStateObjectData { get; }

		IGadgetStateObjectSerialData KillStateObjectData { get; }

		IGadgetStateObjectSerialData TakeoffStateObjectData { get; }

		IGadgetStateObjectSerialData RespawnStateObjectData { get; }

		IGadgetStateObjectSerialData BombStateObjectData { get; }

		IGadgetStateObjectSerialData GBoostStateObjectData { get; }

		IGadgetStateObjectSerialData G0StateObjectData { get; }

		IGadgetStateObjectSerialData G1StateObjectData { get; }

		IGadgetStateObjectSerialData G2StateObjectData { get; }

		IGadgetStateObjectSerialData GPStateObjectData { get; }

		IGadgetStateObjectSerialData GOutOfCombatStateObjectData { get; }

		IGadgetStateObjectSerialData GGStateObjectData { get; }

		IGadgetStateObjectSerialData GTStateObjectData { get; }

		float JokerBarValue { get; }

		float JokerBarMaxValue { get; }

		IGadgetStateObjectSerialData SprayStateObjectData { get; }
	}
}
