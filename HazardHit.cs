using System;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines
{
	public class HazardHit
	{
		public HazardHit(IHazard hazard)
		{
			this._hazard = hazard;
		}

		public void TryHit(ICombatObject target)
		{
			if (target != null && target.Team != this._hazard.Team && (!this._hazard.ShouldHitOnlyBombCarrier || target.IsCarryingBomb))
			{
				this._hazard.HitTarget();
			}
		}

		private readonly IHazard _hazard;
	}
}
