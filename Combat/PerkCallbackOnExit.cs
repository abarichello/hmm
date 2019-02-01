using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkCallbackOnExit : BasePerk
	{
		public override void PerkInitialized()
		{
		}

		protected virtual void OnTriggerExit(Collider other)
		{
			if (this.Effect.IsDead)
			{
				return;
			}
			CombatObject combatObject = null;
			bool flag = false;
			PerkCallbackOnExit.ModeEnum mode = this.Mode;
			if (mode != PerkCallbackOnExit.ModeEnum.None)
			{
				if (mode != PerkCallbackOnExit.ModeEnum.TargetOnly)
				{
					if (mode == PerkCallbackOnExit.ModeEnum.Scenery)
					{
						if (other.gameObject.layer == 9)
						{
							combatObject = this.Effect.GetTargetCombat(BasePerk.PerkTarget.Target);
							flag = true;
						}
					}
				}
				else
				{
					CombatObject combat;
					combatObject = (combat = CombatRef.GetCombat(other));
					flag = (combat && combat.Id == this.Effect.Target && this.Effect.CheckHit(combat));
				}
			}
			else
			{
				CombatObject combat;
				combatObject = (combat = CombatRef.GetCombat(other));
				flag = this.Effect.CheckHit(combat);
			}
			if (!flag)
			{
				return;
			}
			if (combatObject)
			{
				this.ExecuteCallback(combatObject);
			}
		}

		protected virtual void ExecuteCallback(CombatObject callbackTarget)
		{
			if (this.Effect.Gadget is TriggerExitCallback.ITriggerExitCallbackListener)
			{
				((TriggerExitCallback.ITriggerExitCallbackListener)this.Effect.Gadget).OnTriggerExitCallback(new TriggerExitCallback(callbackTarget, this.Effect.Gadget, this.Effect.Data));
			}
		}

		public PerkCallbackOnExit.ModeEnum Mode = PerkCallbackOnExit.ModeEnum.None;

		public enum ModeEnum
		{
			None = 1,
			TargetOnly,
			Scenery
		}
	}
}
