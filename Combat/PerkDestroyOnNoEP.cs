using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDestroyOnNoEP : BasePerk
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._combatOwner = this.Effect.Gadget.Combat;
		}

		private void Update()
		{
			if (this._combatOwner.Data.EP > 20f)
			{
				return;
			}
			this.Effect.TriggerDestroy(this._combatOwner.Id.ObjId, this.Effect.AttachedTransform.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Lifetime, false);
		}

		private CombatObject _combatOwner;
	}
}
