using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDamageColliderOnTick : BasePerk, PerkAttachToObject.IEffectAttachListener
	{
		public override void PerkInitialized()
		{
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
			this.Colliders = new List<Collider>();
			this._timedUpdater = new TimedUpdater(this.TickMillis, false, false);
			this._isSanityChecked = true;
			return true;
		}

		private void FixedUpdate()
		{
			if (!this._timedUpdater.ShouldHalt())
			{
				this.DamageColliderArea();
			}
		}

		protected void OnTriggerEnter(Collider other)
		{
			if (!this.SanityCheck())
			{
				return;
			}
			if (this.Colliders.Count < 1 || !this.Colliders.Contains(other))
			{
				this.Colliders.Add(other);
			}
		}

		protected void OnTriggerExit(Collider other)
		{
			if (!this.SanityCheck())
			{
				return;
			}
			this.Colliders.Remove(other);
		}

		private void DamageColliderArea()
		{
			for (int i = 0; i < this.Colliders.Count; i++)
			{
				Collider comp = this.Colliders[i];
				CombatObject combat = CombatRef.GetCombat(comp);
				bool flag = this.Effect.CheckHit(combat);
				if (flag)
				{
					combat.Controller.AddModifiers(this.Effect.Data.Modifiers, this.Effect.Gadget.Combat, this.Effect.EventId, false);
				}
			}
		}

		public void OnAttachEffect(PerkAttachToObject.EffectAttachToTarget msg)
		{
		}

		public int TickMillis;

		private TimedUpdater _timedUpdater;

		private bool _isSanityChecked;

		public Collider Collider;

		public List<Collider> Colliders;
	}
}
