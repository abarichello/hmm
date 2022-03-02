using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkWardConfig : BasePerk, DestroyEffectMessage.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			this.Ward.Id.Register(ObjectId.New(ContentKind.Wards.Byte(), this.Effect.EventId));
			this.Ward.OwnerTeam = this.Effect.Gadget.Combat.Team;
			this.Ward.Controller.AddPassiveModifiers((!this.UseExtraModifiers) ? this.Effect.Data.Modifiers : this.Effect.Data.ExtraModifiers, this.Effect.Gadget.Combat, this.Effect.EventId);
			this.Ward.WardEffect = this.Effect;
			this.Ward.Data.ResetHP();
			this.Ward.Data.ResetEP();
			this.Ward.gameObject.SetActive(true);
		}

		public void OnDestroyEffect(DestroyEffectMessage evt)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			Rigidbody body = this.Ward.Body;
			if (!body)
			{
				return;
			}
			body.velocity = Vector3.zero;
			body.angularVelocity = Vector3.zero;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkWardConfig));

		public CombatObject Ward;

		public bool UseExtraModifiers;
	}
}
