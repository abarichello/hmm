using System;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDestroyOnGadgetUpgraded : BasePerk, DestroyEffectMessage.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			this.Effect.Gadget.ListenToGadgetSetLevel += this.ServerDestroy;
		}

		public void OnDestroyEffect(DestroyEffectMessage evt)
		{
			this.Effect.Gadget.ListenToGadgetSetLevel -= this.ServerDestroy;
		}

		private void ServerDestroy(GadgetBehaviour gadget, string upgradeName, int level)
		{
			this.Effect.TriggerDestroy((!(this.Effect.Target != null)) ? -1 : this.Effect.Target.ObjId, base._trans.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Default, false);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkDestroyOnLifetime));
	}
}
