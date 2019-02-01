using System;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BombExplosionGadget : BasicCannon
	{
		private BombExplosionInfo MyInfo
		{
			get
			{
				return base.Info as BombExplosionInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery += this.BombManagerOnListenToBombDelivery;
		}

		private void BombManagerOnListenToBombDelivery(int causerId, TeamKind scoredTeam, Vector3 deliveryPosition)
		{
			if (this.Combat.Id.ObjId != causerId)
			{
				return;
			}
			this.FireGadget(base.CannonInfo.Effect, ModifierData.CopyData(this._damage), deliveryPosition);
		}

		protected override int FireGadget(FXInfo effect, ModifierData[] modifier, Vector3 origin)
		{
			EffectEvent effectEvent = base.GetEffectEvent(effect);
			effectEvent.Origin = origin;
			effectEvent.Target = GameHubBehaviour.Hub.BombManager.BombMovement.Combat.transform.position;
			effectEvent.TargetId = GameHubBehaviour.Hub.BombManager.BombMovement.Combat.Id.ObjId;
			effectEvent.LifeTime = ((base.LifeTime <= 0f) ? (effectEvent.Range / effectEvent.MoveSpeed) : base.LifeTime);
			effectEvent.Modifiers = modifier;
			base.SetTargetAndDirection(effectEvent);
			this.eventId = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			base.ExistingFiredEffectsAdd(this.eventId);
			GameHubBehaviour.Hub.Events.Effects.ListenToDestroy(this.eventId, new EffectsManager.EffectDestroyed(this.Callback));
			this.Origin = Vector3.zero;
			return this.eventId;
		}

		private void Callback(EffectRemoveEvent data)
		{
			GameHubBehaviour.Hub.Events.Effects.UnlistenToDestroy(this.eventId, new EffectsManager.EffectDestroyed(this.Callback));
		}

		protected override void OnDestroy()
		{
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery -= this.BombManagerOnListenToBombDelivery;
			base.OnDestroy();
		}

		private int eventId;
	}
}
