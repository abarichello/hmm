using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDestroyOnDamageEvent : BasePerk, DestroyEffect.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			this._combat = base.GetTargetCombat(this.Effect, this.Target);
			if (!this._combat)
			{
				return;
			}
			this.RegisterListener(this._combat);
		}

		public void OnDestroyEffect(DestroyEffect evt)
		{
			if (!this._combat)
			{
				return;
			}
			this.UnRegisterListener(this._combat);
		}

		private void RegisterListener(CombatObject combat)
		{
			switch (this.Event)
			{
			case PerkDestroyOnDamageEvent.DamageEvent.PreDamageCaused:
				combat.ListenToPreDamageCaused += this.OnPreDamageCaused;
				break;
			case PerkDestroyOnDamageEvent.DamageEvent.PreDamageTaken:
				combat.ListenToPreDamageTaken += this.OnPreDamageTaken;
				break;
			case PerkDestroyOnDamageEvent.DamageEvent.PosDamageCaused:
				combat.ListenToPosDamageCaused += this.OnPosDamageCaused;
				break;
			case PerkDestroyOnDamageEvent.DamageEvent.PosDamageTaken:
				combat.ListenToPosDamageTaken += this.OnPosDamageTaken;
				break;
			}
		}

		private void UnRegisterListener(CombatObject combat)
		{
			switch (this.Event)
			{
			case PerkDestroyOnDamageEvent.DamageEvent.PreDamageCaused:
				combat.ListenToPreDamageCaused -= this.OnPreDamageCaused;
				break;
			case PerkDestroyOnDamageEvent.DamageEvent.PreDamageTaken:
				combat.ListenToPreDamageTaken -= this.OnPreDamageTaken;
				break;
			case PerkDestroyOnDamageEvent.DamageEvent.PosDamageCaused:
				combat.ListenToPosDamageCaused -= this.OnPosDamageCaused;
				break;
			case PerkDestroyOnDamageEvent.DamageEvent.PosDamageTaken:
				combat.ListenToPosDamageTaken -= this.OnPosDamageTaken;
				break;
			}
		}

		private void OnPreDamageCaused(CombatObject causer, CombatObject taker, ModifierData mod, ref float amount, int eventid)
		{
			if (causer == this._combat && mod.Info.Effect == this.EffectKind)
			{
				this.Effect.TriggerDestroy(-1, base.transform.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Default, false);
			}
		}

		private void OnPreDamageTaken(CombatObject causer, CombatObject taker, ModifierData mod, ref float amount, int eventId)
		{
			if (taker == this._combat && mod.Info.Effect == this.EffectKind)
			{
				this.Effect.TriggerDestroy(-1, base.transform.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Default, false);
			}
		}

		private void OnPosDamageCaused(CombatObject causer, CombatObject taker, ModifierData mod, float amount, int eventId)
		{
			if (causer == this._combat && mod.Info.Effect == this.EffectKind)
			{
				this.Effect.TriggerDestroy(-1, base.transform.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Default, false);
			}
		}

		private void OnPosDamageTaken(CombatObject causer, CombatObject taker, ModifierData mod, float amount, int eventId)
		{
			if (taker == this._combat && mod.Info.Effect == this.EffectKind)
			{
				this.Effect.TriggerDestroy(-1, base.transform.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Default, false);
			}
		}

		public EffectKind EffectKind;

		public BasePerk.PerkTarget Target;

		public PerkDestroyOnDamageEvent.DamageEvent Event;

		private CombatObject _combat;

		public enum DamageEvent
		{
			PreDamageCaused,
			PreDamageTaken,
			PosDamageCaused,
			PosDamageTaken
		}
	}
}
