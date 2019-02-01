using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BasicCannon : GadgetBehaviour
	{
		public BasicCannonInfo CannonInfo
		{
			get
			{
				return base.Info as BasicCannonInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._damage = ModifierData.CreateData(this.CannonInfo.Damage, this.CannonInfo);
			this._moveSpeed = new Upgradeable(this.CannonInfo.MoveSpeedUpgrade, this.CannonInfo.MoveSpeed, this.CannonInfo.UpgradesValues);
			this.ExtraMoveSpeed = new Upgradeable(this.CannonInfo.ExtraMoveSpeedUpgrade, this.CannonInfo.ExtraMoveSpeed, this.CannonInfo.UpgradesValues);
			base.Pressed = false;
			this.CurrentCooldownTime = 0L;
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._damage.SetLevel(upgradeName, level);
			this._moveSpeed.SetLevel(upgradeName, level);
			this.ExtraMoveSpeed.SetLevel(upgradeName, level);
		}

		protected override int FireGadget()
		{
			if (this.CannonInfo.UseLastWarmupPosition)
			{
				this.Origin = this.LastWarmupPosition;
			}
			else
			{
				this.Origin = GadgetBehaviour.DummyPosition(this.Combat, this.CannonInfo.Effect);
			}
			ModifierData[] modifier = ModifierData.CopyData(this._damage);
			return this.FireGadget(this.CannonInfo.Effect, modifier, this.Origin);
		}

		protected virtual int FireGadget(FXInfo effect, ModifierData[] modifier, Vector3 origin)
		{
			if (string.IsNullOrEmpty(effect.Effect))
			{
				return -1;
			}
			EffectEvent effectEvent = base.GetEffectEvent(effect);
			effectEvent.MoveSpeed = this._moveSpeed.Get();
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = origin;
			effectEvent.Target = base.Target;
			effectEvent.TargetId = this.TargetId;
			float lifeTime;
			if (this.Kind == GadgetKind.Charged)
			{
				lifeTime = this.CannonInfo.LifeTimeCurve.Evaluate(this.CurrentHeat) * base.LifeTime;
			}
			else if (base.LifeTime > 0f)
			{
				lifeTime = base.LifeTime;
			}
			else
			{
				lifeTime = effectEvent.Range / effectEvent.MoveSpeed;
			}
			effectEvent.LifeTime = lifeTime;
			effectEvent.Modifiers = modifier;
			effectEvent.ExtraModifiers = ModifierData.CopyData(this.ExtraModifier);
			base.SetTargetAndDirection(effectEvent);
			int num = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			base.ExistingFiredEffectsAdd(num);
			this.Origin = Vector3.zero;
			return num;
		}

		protected override int FireExtraGadget()
		{
			return this.FireExtraGadget(this.CannonInfo.ExtraEffect, ModifierData.CopyData(this.ExtraModifier), null);
		}

		protected override int FireExtraGadget(Action<EffectEvent> customizeData)
		{
			return this.FireExtraGadget(this.CannonInfo.ExtraEffect, ModifierData.CopyData(this.ExtraModifier), null, customizeData);
		}

		protected override int FireExtraGadget(Vector3 position)
		{
			return this.FireExtraGadget(this.CannonInfo.ExtraEffect, ModifierData.CopyData(this.ExtraModifier), null, delegate(EffectEvent data)
			{
				data.Origin = position;
			});
		}

		protected int FireExtraGadget(FXInfo effect, ModifierData[] modifierData, ModifierData[] modifierData2)
		{
			return this.FireExtraGadget(effect, modifierData, modifierData2, null);
		}

		protected virtual int FireExtraGadget(FXInfo effect, ModifierData[] modifierData, ModifierData[] modifierData2, Action<EffectEvent> customizeData)
		{
			EffectEvent effectEvent = base.GetEffectEvent(effect);
			effectEvent.MoveSpeed = ((this.ExtraMoveSpeed.Get() == 0f) ? this._moveSpeed.Get() : this.ExtraMoveSpeed.Get());
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = GadgetBehaviour.DummyPosition(this.Combat, effect);
			effectEvent.Target = base.Target;
			effectEvent.TargetId = this.TargetId;
			effectEvent.LifeTime = ((this.ExtraLifeTime <= 0f) ? (effectEvent.Range / effectEvent.MoveSpeed) : this.ExtraLifeTime);
			effectEvent.Modifiers = modifierData;
			effectEvent.ExtraModifiers = modifierData2;
			base.SetTargetAndDirection(effectEvent);
			if (customizeData != null)
			{
				customizeData(effectEvent);
			}
			int num = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			base.ExistingFiredEffectsAdd(num);
			return num;
		}

		public override void OnTriggerEnterCallback(TriggerEnterCallback evt)
		{
			if (!this.FireExtraOnEnterCallback.BoolGet())
			{
				return;
			}
			this.FireExtraGadget(evt.Combat.Transform.position);
		}

		public override float GetDps()
		{
			return base.GetDpsFromModifierData(this._damage);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BasicCannon));

		protected ModifierData[] _damage;

		protected Upgradeable _moveSpeed;

		protected Upgradeable ExtraMoveSpeed;
	}
}
