using System;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkCallbackOnAction : BasePerk, DestroyEffectMessage.IDestroyEffectListener
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
			Mural.Post(new ActionCallback(this.Effect.EventId), this.Effect.Gadget);
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
			Mural.Post(new ActionCallback(this.Effect.EventId), this.Effect.Gadget);
		}

		public void OnDestroyEffect(DestroyEffectMessage evt)
		{
			this.Controller.ListenToCancelAction -= this.OnCancelAction;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkCallbackOnAction));

		public BasePerk.PerkTarget Target;

		public bool AllowMovement;

		private IPlayerController _playerController;
	}
}
