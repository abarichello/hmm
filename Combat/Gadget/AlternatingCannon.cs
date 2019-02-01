using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class AlternatingCannon : BasicCannon
	{
		private AlternatingCannonInfo MyInfo
		{
			get
			{
				return base.Info as AlternatingCannonInfo;
			}
		}

		protected override int FireGadget()
		{
			this._lifeTimeStart = -1L;
			this._secondaryDeathTime = -1L;
			return this._currentPrimary = this.FireEffectWithLifeTime(this.MyInfo.Effect, GadgetBehaviour.DummyPosition(this.Combat, this.MyInfo.Effect));
		}

		private int FireEffectWithLifeTime(FXInfo effect, Vector3 origin)
		{
			float num = base.LifeTime;
			if (this._lifeTimeStart > 0L)
			{
				num -= (float)((long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() - this._lifeTimeStart) / 1000f;
			}
			else
			{
				this._lifeTimeStart = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			}
			if (num <= 0f)
			{
				return -1;
			}
			EffectEvent effectEvent = base.GetEffectEvent(effect);
			effectEvent.MoveSpeed = this._moveSpeed.Get();
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = origin;
			effectEvent.Target = base.Target;
			effectEvent.TargetId = this.TargetId;
			effectEvent.LifeTime = num;
			effectEvent.Modifiers = ModifierData.CopyData(this._damage);
			effectEvent.ExtraModifiers = ModifierData.CopyData(this.ExtraModifier);
			base.SetTargetAndDirection(effectEvent);
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		protected override int FireExtraGadget()
		{
			if ((float)((long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() - this._secondaryDeathTime) / 1000f < this.MyInfo.SwitchDelay)
			{
				return -1;
			}
			if (this._currentPrimary > 0)
			{
				BaseFX baseFx = GameHubBehaviour.Hub.Events.Effects.GetBaseFx(this._currentPrimary);
				baseFx.TriggerDefaultDestroy(-1);
				this._currentSecondary = this.FireEffectWithLifeTime(this.MyInfo.ExtraEffect, baseFx.transform.position);
				base.ExistingFiredEffectsAdd(this._currentSecondary);
				return this._currentSecondary;
			}
			if (this._currentSecondary > 0 && this.MyInfo.CanSwitchExtraToNormal)
			{
				BaseFX baseFx2 = GameHubBehaviour.Hub.Events.Effects.GetBaseFx(this._currentSecondary);
				baseFx2.TriggerDefaultDestroy(-1);
				return -1;
			}
			return -1;
		}

		protected override void OnMyEffectDestroyed(DestroyEffect evt)
		{
			if (evt.RemoveData.TargetEventId == this._currentSecondary)
			{
				this._currentPrimary = this.FireEffectWithLifeTime(this.MyInfo.Effect, evt.RemoveData.Origin);
				base.ExistingFiredEffectsAdd(this._currentPrimary);
				this._currentSecondary = -1;
				this._secondaryDeathTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			}
			if (evt.RemoveData.TargetEventId == this._currentPrimary)
			{
				if (this.Combat.IsAlive())
				{
					this._currentSecondary = this.FireEffectWithLifeTime(this.MyInfo.ExtraEffect, evt.RemoveData.Origin);
					base.ExistingFiredEffectsAdd(this._currentSecondary);
				}
				this._currentPrimary = -1;
			}
		}

		private int _currentPrimary = -1;

		private int _currentSecondary = -1;

		private long _lifeTimeStart = -1L;

		private long _secondaryDeathTime = -1L;
	}
}
