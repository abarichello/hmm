using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class MouseDelayedProjectile : BasicCannon
	{
		private MouseDelayedProjectileInfo MyInfo
		{
			get
			{
				return base.Info as MouseDelayedProjectileInfo;
			}
		}

		protected override int FireWarmup()
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.WarmupEffect);
			effectEvent.Origin = this.DummyPosition(effectEvent);
			effectEvent.LifeTime = this.MyInfo.WarmupSeconds;
			base.SetTargetAndDirection(effectEvent);
			this._lastWarmupDirection = effectEvent.Direction;
			this.LastWarmupId = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			return this.LastWarmupId;
		}

		protected override int FireGadget()
		{
			return this.FireGadget(this.MyInfo.Effect, this._damage, this.ExtraModifier);
		}

		protected virtual int FireGadget(FXInfo effect, ModifierData[] modifiers, ModifierData[] extraModifiers)
		{
			EffectEvent effectEvent = base.GetEffectEvent(effect);
			effectEvent.Range = this.GetRange();
			effectEvent.MoveSpeed = this._moveSpeed;
			effectEvent.LifeTime = effectEvent.Range / effectEvent.MoveSpeed;
			effectEvent.Modifiers = modifiers;
			effectEvent.ExtraModifiers = extraModifiers;
			if (this.MyInfo.WarmupSeconds != 0f)
			{
				if (this.MyInfo.UseLastWarmupPosition)
				{
					effectEvent.Origin = this.LastWarmupPosition;
				}
				else
				{
					effectEvent.Origin = GadgetBehaviour.DummyPosition(this.Combat, base.CannonInfo.Effect);
				}
				if (this.MyInfo.ReEvaluateTargetOnFire)
				{
					base.SetTargetAndDirection(effectEvent);
				}
				else
				{
					effectEvent.Direction = this._lastWarmupDirection;
					effectEvent.Target = effectEvent.Origin + effectEvent.Direction * effectEvent.Range;
				}
			}
			else
			{
				effectEvent.Origin = this.DummyPosition(effectEvent);
				base.SetTargetAndDirection(effectEvent);
			}
			this._extraEffectOrigin = effectEvent.Origin;
			this._extraEffectTarget = effectEvent.Target;
			this._extraEffectDirection = effectEvent.Direction;
			if (this.FireNormalAndExtraEffectsTogether.BoolGet())
			{
			}
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		protected virtual int FireGadgetFromTo(Vector3 from, Vector3 to, ModifierData[] modifiers, FXInfo effect)
		{
			EffectEvent effectEvent = base.GetEffectEvent(effect);
			effectEvent.Origin = from;
			effectEvent.Target = to;
			effectEvent.Range = (to - from).magnitude;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target);
			effectEvent.MoveSpeed = this._moveSpeed;
			effectEvent.LifeTime = effectEvent.Range / effectEvent.MoveSpeed;
			effectEvent.Modifiers = modifiers;
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		protected virtual int FireGadgetFromTo(Vector3 from, Vector3 to, ModifierData[] modifiers)
		{
			return this.FireGadgetFromTo(from, to, modifiers, this.MyInfo.Effect);
		}

		protected virtual int FireExtraWarmup()
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.ExtraWarmupEffect);
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = this._extraEffectOrigin;
			effectEvent.Target = this._extraEffectTarget;
			effectEvent.Direction = this._extraEffectDirection;
			effectEvent.LifeTime = this.MyInfo.ExtraWarmupSeconds;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target);
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		protected override int FireExtraGadget()
		{
			return this.FireExtraGadget(this._extraEffectOrigin);
		}

		protected override int FireExtraGadget(Vector3 position)
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.ExtraEffect);
			effectEvent.Range = this.GetRange();
			effectEvent.MoveSpeed = ((this.ExtraMoveSpeed.Get() == 0f) ? this._moveSpeed.Get() : this.ExtraMoveSpeed.Get());
			effectEvent.LifeTime = ((base.Info.ExtraLifeTime == 0f) ? (effectEvent.Range / effectEvent.MoveSpeed) : base.Info.ExtraLifeTime);
			effectEvent.Origin = position;
			effectEvent.Target = this._extraEffectTarget;
			effectEvent.Direction = this._extraEffectDirection;
			effectEvent.Modifiers = this.ExtraModifier;
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		protected override void InnerOnDestroyEffect(DestroyEffect evt)
		{
			base.InnerOnDestroyEffect(evt);
			if (this.CurrentExtraWarmupEffectId == evt.RemoveData.TargetEventId)
			{
				this.CurrentExtraEffectId = this.FireExtraGadget();
				this.CurrentExtraWarmupEffectId = -1;
			}
		}

		protected int CurrentExtraEffectId = -1;

		protected int CurrentExtraWarmupEffectId = -1;

		private Vector3 _lastWarmupDirection;

		private Vector3 _extraEffectOrigin;

		private Vector3 _extraEffectTarget;

		private Vector3 _extraEffectDirection;
	}
}
