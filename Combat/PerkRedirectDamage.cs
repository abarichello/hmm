using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkRedirectDamage : BasePerk, DestroyEffect.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._fromCombatObject = this.Effect.GetTargetCombat(this.From);
			this._toCombatObject = this.Effect.GetTargetCombat(this.To);
			if (this._fromCombatObject == null)
			{
				return;
			}
			if (this._toCombatObject == null)
			{
				return;
			}
			if (!this.Effect.CheckHit(this._fromCombatObject))
			{
				return;
			}
			if (!this.Effect.CheckHit(this._toCombatObject))
			{
				return;
			}
			this.AddListener(this._fromCombatObject);
		}

		public void OnDestroyEffect(DestroyEffect evt)
		{
			if (this._fromCombatObject)
			{
				this.RemoveListener(this._fromCombatObject);
			}
		}

		private void AddListener(CombatObject obj)
		{
			obj.ListenToPreDamageTaken += this.OnPreDamageTaken;
		}

		private void RemoveListener(CombatObject obj)
		{
			obj.ListenToPreDamageTaken -= this.OnPreDamageTaken;
		}

		private void OnPreDamageTaken(CombatObject causer, CombatObject taker, ModifierData data, ref float amount, int eventId)
		{
			if (eventId == this.Effect.EventId)
			{
				return;
			}
			if (this.IgnoreReactiveModifiers && data.IsReactive)
			{
				return;
			}
			if (!Array.Exists<EffectKind>(this.DamageKinds, (EffectKind x) => data.Info.Effect == x))
			{
				return;
			}
			if (!this._toCombatObject.IsAlive())
			{
				return;
			}
			this._toCombatObject.Controller.AddModifier(data, causer, eventId, false);
			amount = 0f;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkRedirectDamage));

		private CombatObject _fromCombatObject;

		private CombatObject _toCombatObject;

		public BasePerk.PerkTarget From;

		public BasePerk.PerkTarget To = BasePerk.PerkTarget.Target;

		public EffectKind[] DamageKinds;

		public bool IgnoreReactiveModifiers = true;
	}
}
