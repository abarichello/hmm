using System;
using System.Collections;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class FireOtherGadgetOverTime : BasicCannon
	{
		private FireOtherGadgetOverTimeInfo MyInfo
		{
			get
			{
				return base.Info as FireOtherGadgetOverTimeInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this.Duration = this.MyInfo.duration;
			this.Ticks = this.MyInfo.tick;
			this.OtherGadget = this.Combat.GetGadget(this.MyInfo.gadget);
			this._disableMod = this.GetDisableMod();
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery += this.OnBombDelivery;
		}

		protected override int FireGadget()
		{
			if (this._disableMod != null)
			{
				this.Combat.Controller.AddPassiveModifier(this._disableMod, this.Combat, -1);
			}
			base.StartCoroutine(this.FiringGadget());
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.TokenFX);
			effectEvent.LifeTime = this.Duration;
			int result = -1;
			if (!string.IsNullOrEmpty(effectEvent.EffectInfo.Effect))
			{
				result = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			}
			return result;
		}

		protected IEnumerator FiringGadget()
		{
			float duration = this.Duration;
			float ticks = 0f;
			EffectEvent decorationEvent = base.GetEffectEvent(this.MyInfo.FirstEffectDecoration);
			this.Combat.GadgetStates.SetEffectState(this._gadgetState, EffectState.Running);
			while (duration > 0f && this.Combat.IsAlive() && this.Combat.GadgetStates.GetGadgetState(base.Slot).EffectState == EffectState.Running)
			{
				if (this.Combat.Attributes.IsGadgetDisarmed(base.Slot, base.Nature))
				{
					yield return UnityUtils.WaitForEndOfFrame;
				}
				else
				{
					if (ticks <= 0f)
					{
						decorationEvent.Origin = this.DummyPosition();
						decorationEvent.LifeTime = this.MyInfo.EffectDecorationDuration;
						if (!string.IsNullOrEmpty(decorationEvent.EffectInfo.Effect))
						{
							GameHubBehaviour.Hub.Events.TriggerEvent(decorationEvent);
						}
						this.OtherGadget.ForceFire();
						ticks = this.GetTick();
						decorationEvent = base.GetEffectEvent(this.MyInfo.EffectsDecoration);
					}
					yield return UnityUtils.WaitForEndOfFrame;
					ticks -= Time.deltaTime;
					duration -= Time.deltaTime;
				}
			}
			decorationEvent = base.GetEffectEvent(this.MyInfo.LastEffectDecoration);
			decorationEvent.Origin = this.DummyPosition();
			decorationEvent.LifeTime = this.MyInfo.EffectDecorationDuration;
			if (!string.IsNullOrEmpty(decorationEvent.EffectInfo.Effect))
			{
				GameHubBehaviour.Hub.Events.TriggerEvent(decorationEvent);
			}
			this.StopGadget();
			yield break;
		}

		protected float GetTick()
		{
			CombatAttributes attributes = this.Combat._data.Combat.Attributes;
			switch (this.MyInfo.gadget)
			{
			case GadgetSlot.CustomGadget0:
				return this.Ticks * (1f - attributes.CooldownReductionGadget0Pct) - attributes.CooldownReductionGadget0;
			case GadgetSlot.CustomGadget1:
				return this.Ticks * (1f - attributes.CooldownReductionGadget1Pct) - attributes.CooldownReductionGadget1;
			case GadgetSlot.CustomGadget2:
				return this.Ticks * (1f - attributes.CooldownReductionGadget2Pct) - attributes.CooldownReductionGadget2;
			default:
				return this.Ticks;
			}
		}

		protected ModifierData GetDisableMod()
		{
			ModifierInfo modifierInfo = new ModifierInfo();
			GadgetSlot gadget = this.MyInfo.gadget;
			if (gadget != GadgetSlot.CustomGadget0)
			{
				if (gadget == GadgetSlot.CustomGadget1)
				{
					modifierInfo.Status = StatusKind.Gadget1Disarmed;
				}
			}
			else
			{
				modifierInfo.Status = StatusKind.Gadget0Disarmed;
			}
			modifierInfo.HitOwner = true;
			return new ModifierData(modifierInfo);
		}

		public override void OnObjectUnspawned(UnspawnEvent evt)
		{
			base.OnObjectUnspawned(evt);
			this.StopGadget();
		}

		private void StopGadget()
		{
			if (this._disableMod != null)
			{
				this.Combat.Controller.RemovePassiveModifier(this._disableMod, this.Combat, -1);
			}
			this.Combat.GadgetStates.SetEffectState(this._gadgetState, EffectState.Idle);
		}

		private void OnBombDelivery(int causerid, TeamKind scoredteam, Vector3 deliveryPosition)
		{
			this.StopGadget();
		}

		protected override void RemoveListeners()
		{
			base.RegisterListeners();
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery -= this.OnBombDelivery;
		}

		protected float Duration;

		protected float Ticks;

		protected GadgetBehaviour OtherGadget;

		private ModifierData _disableMod;
	}
}
