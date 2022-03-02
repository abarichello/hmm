using System;
using HeavyMetalMachines.Playback.Snapshot;

namespace HeavyMetalMachines.Combat
{
	public interface IGadgetStateObjectSerialData : IBaseStreamSerialData<IGadgetStateObjectSerialData>
	{
		GadgetState GadgetState { get; }

		EffectState EffectState { get; }

		long Cooldown { get; }

		int Value { get; }

		float Heat { get; }

		int Counter { get; }

		int[] AffectedIds { get; }
	}
}
