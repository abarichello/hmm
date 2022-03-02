using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkCooldownReductionOnDamage : BasePerk, DestroyEffectMessage.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._causerCombatObject = base.GetTargetCombat(this.Effect, this.Causer);
			this._targetCombatObject = base.GetTargetCombat(this.Effect, this.Target);
			if (this._causerCombatObject == null)
			{
				PerkCooldownReductionOnDamage.Log.Debug("Impossible to get From CombatObject");
				return;
			}
			if (this._targetCombatObject == null)
			{
				PerkCooldownReductionOnDamage.Log.Debug("Impossible to get From CombatObject");
				return;
			}
			if (!this.Effect.CheckHit(this._causerCombatObject))
			{
				return;
			}
			if (!this.Effect.CheckHit(this._targetCombatObject))
			{
				return;
			}
			this.AddListener(this._targetCombatObject);
		}

		public void OnDestroyEffect(DestroyEffectMessage evt)
		{
			if (this._targetCombatObject)
			{
				this.RemoveListener(this._targetCombatObject);
			}
		}

		private void AddListener(CombatObject obj)
		{
			obj.ListenToPosDamageTaken += this.OnPosDamageTaken;
		}

		private void RemoveListener(CombatObject obj)
		{
			obj.ListenToPosDamageTaken -= this.OnPosDamageTaken;
		}

		private void OnPosDamageTaken(CombatObject causer, CombatObject taker, ModifierData data, float amount, int eventId)
		{
			if (eventId == this.Effect.EventId)
			{
				return;
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkEqualizeHP));

		private CombatObject _causerCombatObject;

		private CombatObject _targetCombatObject;

		public BasePerk.PerkTarget Causer;

		public BasePerk.PerkTarget Target = BasePerk.PerkTarget.Target;
	}
}
