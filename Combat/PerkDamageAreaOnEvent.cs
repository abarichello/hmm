using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDamageAreaOnEvent : BaseDamageableAreaPerk
	{
		protected override void OnPerkInitialized()
		{
			base.OnPerkInitialized();
			this.RegisterListeners();
		}

		public override void OnDestroyEffect(DestroyEffectMessage evt)
		{
			base.OnDestroyEffect(evt);
			this.RemoveListeners();
		}

		protected virtual void OnEventDamage(CombatObject causer, CombatObject taker, ModifierData mod, float amount, int eventId)
		{
			if (this._lastEventIdReceived == eventId || this.TargetEffectKind != mod.Info.Effect)
			{
				return;
			}
			this._lastEventIdReceived = eventId;
			this.DamageArea(base.transform.position);
		}

		protected virtual void OnEventDamage(CombatObject causer, CombatObject taker, ModifierData mod, ref float amount, int eventId)
		{
			if (this._lastEventIdReceived == eventId)
			{
				return;
			}
			this._lastEventIdReceived = eventId;
			this.DamageArea(base.transform.position);
		}

		protected override void DamageArea(Vector3 position)
		{
			this.GetHits(position, ref this.HittingCombatObjects);
			for (int i = 0; i < this.HittingCombatObjects.Count; i++)
			{
				BarrierUtils.CombatHit combatHit = this.HittingCombatObjects[i];
				CombatObject combat = combatHit.Combat;
				Vector3 normalized = (combat.Transform.position - position).normalized;
				base.ApplyDamage(combat, combat, combatHit.Barrier, normalized, position);
				this.DamagedPlayers.Add(combat);
			}
			if (!this.IsDamageCallbackEnabled)
			{
				return;
			}
			Mural.Post(new DamageAreaCallback(this.DamagedPlayers, position, this.Effect, this.TargetGadgetCallback), this.Effect.Gadget);
			this.DamagedPlayers.Clear();
		}

		protected virtual void RegisterListeners()
		{
			if (!this.TargetCombat)
			{
				return;
			}
			switch (this.ActionKind)
			{
			case CombatObject.ActionKind.OnPreDamageCaused:
				this.TargetCombat.ListenToPreDamageCaused += this.OnEventDamage;
				break;
			case CombatObject.ActionKind.OnPosDamageCaused:
				this.TargetCombat.ListenToPosDamageCaused += this.OnEventDamage;
				break;
			case CombatObject.ActionKind.OnPreDamageTaken:
				this.TargetCombat.ListenToPreDamageTaken += this.OnEventDamage;
				break;
			case CombatObject.ActionKind.OnPosDamageTaken:
				this.TargetCombat.ListenToPosDamageTaken += this.OnEventDamage;
				break;
			case CombatObject.ActionKind.OnPreHealingCaused:
				this.TargetCombat.ListenToPreHealingCaused += this.OnEventDamage;
				break;
			case CombatObject.ActionKind.OnPosHealingCaused:
				this.TargetCombat.ListenToPosHealingCaused += this.OnEventDamage;
				break;
			case CombatObject.ActionKind.OnPreHealingTaken:
				this.TargetCombat.ListenToPreHealingTaken += this.OnEventDamage;
				break;
			case CombatObject.ActionKind.OnPosHealingTaken:
				this.TargetCombat.ListenToPosHealingTaken += this.OnEventDamage;
				break;
			}
		}

		protected virtual void RemoveListeners()
		{
			if (!this.TargetCombat)
			{
				return;
			}
			this.TargetCombat.ListenToPreDamageCaused -= this.OnEventDamage;
			this.TargetCombat.ListenToPosDamageCaused -= this.OnEventDamage;
			this.TargetCombat.ListenToPreDamageTaken -= this.OnEventDamage;
			this.TargetCombat.ListenToPosDamageTaken -= this.OnEventDamage;
			this.TargetCombat.ListenToPreHealingCaused -= this.OnEventDamage;
			this.TargetCombat.ListenToPosHealingCaused -= this.OnEventDamage;
			this.TargetCombat.ListenToPreHealingTaken -= this.OnEventDamage;
			this.TargetCombat.ListenToPosHealingTaken -= this.OnEventDamage;
		}

		public CombatObject.ActionKind ActionKind;

		public EffectKind TargetEffectKind;

		private int _lastEventIdReceived = -1;
	}
}
