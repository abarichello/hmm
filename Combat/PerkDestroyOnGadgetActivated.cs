using System;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDestroyOnGadgetActivated : BasePerk
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._gadgetOwner = base.GetTargetCombat(this.Effect, this.GadgetOwner);
			this._gadget = this._gadgetOwner.GetGadget(this.Gadget);
			this._gadget.ServerListenToWarmupFired += this.OnDestroyed;
		}

		private void OnDestroyed()
		{
			this.Effect.TriggerDestroy(-1, base._trans.position, false, null, Vector3.zero, (!this.SimulateDestroyOnLifetime) ? BaseFX.EDestroyReason.Gadget : BaseFX.EDestroyReason.Lifetime, false);
		}

		public override void PerkDestroyed(DestroyEffectMessage destroyEffectMessage)
		{
			base.PerkDestroyed(destroyEffectMessage);
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this._gadget.ServerListenToWarmupFired -= this.OnDestroyed;
		}

		public BasePerk.PerkTarget GadgetOwner;

		public GadgetSlot Gadget;

		public bool SimulateDestroyOnLifetime;

		private CombatObject _gadgetOwner;

		private GadgetBehaviour _gadget;
	}
}
