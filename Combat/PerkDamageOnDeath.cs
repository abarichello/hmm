using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDamageOnDeath : BaseDamageablePerk
	{
		protected override void OnPerkInitialized()
		{
			this._target = base.GetTargetCombat(this.Effect, this.Target);
			if (this._target)
			{
				if (!this._target.IsAlive())
				{
					this.ServerDestroy();
					return;
				}
				this._target.ListenToObjectUnspawn += this.ApplyMod;
			}
		}

		private void ApplyMod(CombatObject obj, UnspawnEvent msg)
		{
			base.ApplyDamage(obj, obj, false);
		}

		private void ServerDestroy()
		{
			this.Effect.TriggerDestroy(-1, base._trans.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Default, false);
		}

		public override void OnDestroyEffect(DestroyEffect evt)
		{
			base.OnDestroyEffect(evt);
			this.RemoveListeners();
		}

		private void RemoveListeners()
		{
			if (this._target)
			{
				this._target.ListenToObjectUnspawn -= this.ApplyMod;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PerkDamageOnDeath));

		private CombatObject _target;
	}
}
