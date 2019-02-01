using System;
using HeavyMetalMachines.Combat;

namespace HeavyMetalMachines.Frontend
{
	public class HudLifebarCreepObject : HudLifebarObject
	{
		public override void Setup(CombatObject combatObject)
		{
			base.Setup(combatObject);
			this.uiLifeBar.kind = UILifeBar.Kind.Enemy;
		}

		protected override void CombatObjectOnObjectUnspawn(CombatObject combatObject, UnspawnEvent msg)
		{
			base.CombatObjectOnObjectUnspawn(combatObject, msg);
			base.PoolRelease();
		}
	}
}
