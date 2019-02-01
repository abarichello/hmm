using System;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkCancelEffectsByAction : BasePerk, DestroyEffect.IDestroyEffectListener
	{
		private IPlayerController Controller
		{
			get
			{
				IPlayerController result;
				if ((result = this._playerController) == null)
				{
					result = (this._playerController = (this.Effect.GetComponentInTarget<PlayerController>(this.Target, false) ?? this.Effect.GetComponentInTarget<BotAIController>(this.Target, true)));
				}
				return result;
			}
		}

		public override void PerkInitialized()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._playerController = null;
			this.Controller.ListenToCancelAction += this.OnCancelAction;
		}

		private void OnCancelAction(GadgetBehaviour gadgetBehaviour)
		{
			this.Effect.TriggerDestroy(this.Effect.Data.SourceCombat.Id.ObjId, base.transform.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Default, false);
		}

		private void FixedUpdate()
		{
			if (this.AllowMovement)
			{
				return;
			}
			if (!this.Controller.MovingCar)
			{
				return;
			}
			this.Effect.TriggerDestroy(this.Effect.Data.SourceCombat.Id.ObjId, base.transform.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Default, false);
		}

		public void OnDestroyEffect(DestroyEffect evt)
		{
			this.Controller.ListenToCancelAction -= this.OnCancelAction;
		}

		public BasePerk.PerkTarget Target;

		public bool AllowMovement;

		private IPlayerController _playerController;
	}
}
