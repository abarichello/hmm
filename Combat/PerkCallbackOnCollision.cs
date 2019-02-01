using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkCallbackOnCollision : BasePerk
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._target = base.GetTargetCombat(this.Effect, this.Target);
			if (this._target == null)
			{
				PerkCallbackOnCollision.Log.ErrorFormat("{0} - Unable to get CombatObject from target", new object[]
				{
					this.Effect
				});
				return;
			}
			this.AddListener();
		}

		private void AddListener()
		{
			this._target.ListenToCollisionEnter += this.OnCollided;
			if (this.ListenToCollisionStay)
			{
				this._target.ListenToCollisionStay += this.OnCollided;
			}
		}

		private void RemoveListener()
		{
			if (this._target)
			{
				this._target.ListenToCollisionEnter -= this.OnCollided;
				if (this.ListenToCollisionStay)
				{
					this._target.ListenToCollisionStay -= this.OnCollided;
				}
			}
		}

		private void OnCollided(Collision col)
		{
			if (this.Effect.Data == null)
			{
				return;
			}
			CombatObject combat = CombatRef.GetCombat(col.collider);
			if (this.HitScenery && LayerManager.IsSceneryOrBombBlocker(this.Effect, col.collider) && col.collider.attachedRigidbody == null)
			{
				if (this.Effect.Gadget is CollisionCallback.ICollisionCallbackListener)
				{
					((CollisionCallback.ICollisionCallbackListener)this.Effect.Gadget).OnCollisionCallback(new CollisionCallback(this._target, null, true, this.Effect.Gadget, this.Effect.Data, col.contacts[0].normal));
				}
			}
			else if (this.Effect.CheckHit(combat) && this._target != combat && this.Effect.Gadget is CollisionCallback.ICollisionCallbackListener)
			{
				((CollisionCallback.ICollisionCallbackListener)this.Effect.Gadget).OnCollisionCallback(new CollisionCallback(this._target, combat, false, this.Effect.Gadget, this.Effect.Data, col.contacts[0].normal));
			}
		}

		public override void PerkDestroyed(DestroyEffect destroyEffect)
		{
			this.RemoveListener();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PerkCallbackOnCollision));

		private CombatObject _target;

		public BasePerk.PerkTarget Target = BasePerk.PerkTarget.None;

		public bool HitScenery;

		public bool ListenToCollisionStay;
	}
}
