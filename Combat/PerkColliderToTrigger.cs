using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkColliderToTrigger : BasePerk, DestroyEffect.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			this._colliders.Clear();
			Identifiable identifiable = (this.Target != BasePerk.PerkTarget.Owner) ? this.Effect.Target : this.Effect.Owner;
			if (!identifiable)
			{
				return;
			}
			foreach (Collider collider in identifiable.GetComponentsInChildren<Collider>())
			{
				if (collider && !collider.isTrigger)
				{
					collider.isTrigger = true;
					this._colliders.Add(collider);
				}
			}
		}

		public void OnDestroyEffect(DestroyEffect evt)
		{
			for (int i = 0; i < this._colliders.Count; i++)
			{
				Collider collider = this._colliders[i];
				collider.isTrigger = false;
			}
			this._colliders.Clear();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkColliderToTrigger));

		public BasePerk.PerkTarget Target;

		private List<Collider> _colliders = new List<Collider>();
	}
}
