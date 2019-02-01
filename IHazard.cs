using System;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines
{
	public interface IHazard
	{
		void HitTarget();

		TeamKind Team { get; }

		bool ShouldHitOnlyBombCarrier { get; }
	}
}
