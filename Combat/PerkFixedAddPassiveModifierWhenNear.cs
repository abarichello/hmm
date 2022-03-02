using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[RequireComponent(typeof(Collider))]
	public class PerkFixedAddPassiveModifierWhenNear : BasePerk, DestroyEffectMessage.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			this._isSanityChecked = false;
			this.SanityCheck();
		}

		private bool SanityCheck()
		{
			if (this._isSanityChecked)
			{
				return true;
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return false;
			}
			this._targetCombat = base.GetTargetCombat(this.Effect, this.Target);
			this._triggerCombats = new List<CombatObject>();
			this.Modifier.SetInfo(this.Modifier.Info, this.Effect.Gadget.Info);
			this._isSanityChecked = true;
			return true;
		}

		protected void OnTriggerEnter(Collider triggerCollider)
		{
			if (!this.SanityCheck())
			{
				return;
			}
			CombatObject combat = CombatRef.GetCombat(triggerCollider);
			bool flag = this.Effect.CheckHit(combat);
			if (combat && flag && !this._triggerCombats.Contains(combat))
			{
				this._triggerCombats.Add(combat);
				if (this._triggerCombats.Count == 1)
				{
					this._targetCombat.Controller.AddPassiveModifier(this.Modifier, this.Effect.Gadget.Combat, this.Effect.EventId);
				}
			}
		}

		protected void OnTriggerExit(Collider triggerCollider)
		{
			if (!this.SanityCheck())
			{
				return;
			}
			CombatObject combat = CombatRef.GetCombat(triggerCollider);
			bool flag = this.Effect.CheckHit(combat);
			if (combat && flag && this._triggerCombats.Contains(combat))
			{
				this._triggerCombats.Remove(combat);
				if (this._triggerCombats.Count == 0)
				{
					this._targetCombat.Controller.RemovePassiveModifier(this.Modifier, this.Effect.Gadget.Combat, this.Effect.EventId);
				}
			}
		}

		public void OnDestroyEffect(DestroyEffectMessage evt)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (this._triggerCombats.Count > 0)
			{
				this._targetCombat.Controller.RemovePassiveModifier(this.Modifier, this.Effect.Gadget.Combat, this.Effect.EventId);
			}
		}

		public ModifierData Modifier;

		public BasePerk.PerkTarget Target;

		private CombatObject _targetCombat;

		private List<CombatObject> _triggerCombats;

		private bool _isSanityChecked;
	}
}
