using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDestroyOnDeath : BasePerk, DestroyEffectMessage.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			CombatObject combat = CombatRef.GetCombat(this.Effect.Target);
			if (combat && this.Target)
			{
				if (!combat.IsAlive())
				{
					this.ServerDestroy(combat, default(UnspawnEvent));
					return;
				}
				combat.ListenToObjectUnspawn += this.ServerDestroy;
			}
			CombatObject combat2 = CombatRef.GetCombat(this.Effect.Owner);
			if (combat2 && this.Owner)
			{
				if (!combat2.IsAlive())
				{
					this.ServerDestroy(combat2, default(UnspawnEvent));
					return;
				}
				combat2.ListenToObjectUnspawn += this.ServerDestroy;
			}
		}

		private void ServerDestroy(CombatObject obj, UnspawnEvent msg)
		{
			this.Effect.TriggerDestroy(-1, base._trans.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Default, false);
		}

		public void OnDestroyEffect(DestroyEffectMessage evt)
		{
			this.RemoveListeners();
		}

		private void RemoveListeners()
		{
			CombatObject combat = CombatRef.GetCombat(this.Effect.Target);
			if (combat && this.Target)
			{
				combat.ListenToObjectUnspawn -= this.ServerDestroy;
			}
			CombatObject combat2 = CombatRef.GetCombat(this.Effect.Owner);
			if (combat2 && this.Owner)
			{
				combat2.ListenToObjectUnspawn -= this.ServerDestroy;
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkDestroyOnDeath));

		public bool Owner;

		public bool Target;
	}
}
