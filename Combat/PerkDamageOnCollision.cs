using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDamageOnCollision : BasePerk, DestroyEffect.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._hits.Clear();
			this._listening = base.GetTargetCombat(this.Effect, this.Target);
			if (this._listening == null)
			{
				PerkDamageOnCollision.Log.Warn("PerkDamageOnCollision - Unable to get CombatObject from target");
				return;
			}
			if (!this.Effect.CheckHit(this._listening))
			{
				return;
			}
			this.AddListener(this._listening);
			this._modifiers = ((!this.UseExtraModifiers) ? this.Effect.Data.Modifiers : this.Effect.Data.ExtraModifiers);
		}

		public void OnDestroyEffect(DestroyEffect evt)
		{
			if (this._listening)
			{
				this.RemoveListener(this._listening);
			}
			this._hits.Clear();
		}

		private void AddListener(CombatObject obj)
		{
			if (this.UseCollisionEnter)
			{
				obj.ListenToCollisionEnter += this.OnCollided;
			}
			else
			{
				obj.ListenToCollisionStay += this.OnCollided;
			}
		}

		private void RemoveListener(CombatObject obj)
		{
			if (this.UseCollisionEnter)
			{
				obj.ListenToCollisionEnter -= this.OnCollided;
			}
			else
			{
				obj.ListenToCollisionStay -= this.OnCollided;
			}
		}

		private void OnCollided(Collision col)
		{
			CombatObject combat = CombatRef.GetCombat(col.rigidbody.GetComponent<Collider>());
			if (!combat)
			{
				return;
			}
			if (this.UseCollisionEnter)
			{
				combat.Controller.AddModifiers(this._modifiers, this._listening, -1, false);
				return;
			}
			int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			int num;
			if (this._hits.TryGetValue(combat, out num) && num > playbackTime)
			{
				return;
			}
			combat.Controller.AddModifiers(this._modifiers, this._listening, -1, false);
			this._hits[combat] = playbackTime + (int)(this.HitFrequencySeconds * 1000f);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PerkDamageOnCollision));

		private CombatObject _listening;

		public BasePerk.PerkTarget Target;

		public bool UseExtraModifiers;

		public bool UseCollisionEnter;

		public float HitFrequencySeconds = 0.25f;

		private ModifierData[] _modifiers;

		private readonly Dictionary<CombatObject, int> _hits = new Dictionary<CombatObject, int>();
	}
}
