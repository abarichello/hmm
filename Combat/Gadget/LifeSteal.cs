using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class LifeSteal : BasicCannon
	{
		private LifeStealInfo MyInfo
		{
			get
			{
				return base.Info as LifeStealInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this.DrainLifeAuraRange = new Upgradeable(this.MyInfo.DrainLifeAuraRangeUpgrade, this.MyInfo.DrainLifeAuraRange, this.MyInfo.UpgradesValues);
			this.DrainLifeAuraPct = new Upgradeable(this.MyInfo.DrainLifeAuraPctUpgrade, this.MyInfo.DrainLifeAuraPct, this.MyInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this.DrainLifeAuraRange.SetLevel(upgradeName, level);
			this.DrainLifeAuraPct.SetLevel(upgradeName, level);
		}

		protected override void OnPosDamageCaused(CombatObject causer, CombatObject taker, ModifierData mod, float amount, int eventId)
		{
			this.GetHittingCombatsInArea(this.Combat.transform.position, this.DrainLifeAuraRange, 1077054464, this.MyInfo.Effect, ref this.m_cpoCombatObjects);
			for (int i = 0; i < this.m_cpoCombatObjects.Count; i++)
			{
				CombatObject combatObject = this.m_cpoCombatObjects[i];
				if (combatObject.Id.ObjId != this.Combat.Id.ObjId && mod.GadgetInfo.GadgetId == base.Info.GadgetId)
				{
					if (combatObject.Team == this.Combat.Team)
					{
						base.DrainLifeCheck(combatObject, taker, mod.Info.Effect.IsHPDamage(), amount, this.DrainLifeAuraPct, eventId, this.MyInfo.DrainLifeAuraFeedback);
					}
				}
			}
		}

		protected Upgradeable DrainLifeAuraRange;

		protected Upgradeable DrainLifeAuraPct;

		private List<CombatObject> m_cpoCombatObjects = new List<CombatObject>(20);
	}
}
