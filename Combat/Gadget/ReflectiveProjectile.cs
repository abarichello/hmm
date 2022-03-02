using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class ReflectiveProjectile : BasicCannon
	{
		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._reflectModifiers = ModifierData.CreateData(this.MyInfo.ReflectModifiers, this.MyInfo);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._reflectModifiers.SetLevel(upgradeName, level);
		}

		private ReflectiveProjectileInfo MyInfo
		{
			get
			{
				return base.Info as ReflectiveProjectileInfo;
			}
		}

		protected override int FireGadget()
		{
			this._endEffectId = -1;
			this._maxReflections = 0;
			int result = base.FireGadget();
			this._totalDistance = 0f;
			return result;
		}

		protected int FireGadgetFromTo(Vector3 from, Vector3 to, ModifierData[] modifiers)
		{
			this._maxReflections = 0;
			int num = this.FireGadgetFromTo(from, to, modifiers, this.MyInfo.Effect);
			base.ExistingFiredEffectsAdd(num);
			this._totalDistance = 0f;
			return num;
		}

		protected int FireGadgetFromTo(Vector3 from, Vector3 to, ModifierData[] modifiers, FXInfo effect)
		{
			EffectEvent effectEvent = base.GetEffectEvent(effect);
			effectEvent.Origin = from;
			effectEvent.Target = to;
			effectEvent.Range = (to - from).magnitude;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target);
			effectEvent.MoveSpeed = this._moveSpeed;
			effectEvent.LifeTime = effectEvent.Range / effectEvent.MoveSpeed;
			effectEvent.Modifiers = modifiers;
			int num = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			this._maxReflections = 0;
			this._totalDistance = 0f;
			base.ExistingFiredEffectsAdd(num);
			return num;
		}

		public override bool CanDestroyEffect(ref EffectRemoveEvent destroy)
		{
			if (destroy.SrvWasScenery && base.ExistingFiredEffectsContains(destroy.TargetEventId))
			{
				BaseFX baseFx = GameHubBehaviour.Hub.Events.Effects.GetBaseFx(destroy.TargetEventId);
				this._latestProjectileRadius = baseFx.GetRadius();
				destroy.WillCreateNextEvent = true;
			}
			return true;
		}

		protected override void InnerOnDestroyEffect(DestroyEffectMessage evt)
		{
			base.InnerOnDestroyEffect(evt);
			this._totalDistance += (evt.RemoveData.Origin - evt.EffectData.Origin).magnitude;
			if (this.MyInfo.IsReflectable && evt.RemoveData.SrvWasScenery)
			{
				int num = this.ReflectEffect(evt, this.MyInfo.ReflectEffect, this._reflectModifiers);
				if (num != -1 && base.ExistingFiredEffectsContains(evt.RemoveData.TargetEventId))
				{
					base.ExistingFiredEffectsRemove(evt.RemoveData.TargetEventId);
					base.ExistingFiredEffectsAdd(num);
				}
			}
			else if (evt.EffectData.EffectInfo.EffectId != this.MyInfo.WarmupEffect.EffectId)
			{
				this.EndEffect(evt, this._totalDistance);
			}
		}

		protected virtual int ReflectEffect(DestroyEffectMessage evt, FXInfo effect, ModifierData[] modifiers)
		{
			if (++this._maxReflections > this.MyInfo.MaxReflections)
			{
				return -1;
			}
			EffectEvent effectEvent = base.GetEffectEvent(effect);
			effectEvent.MoveSpeed = this._moveSpeed;
			evt.RemoveData.SrvNormal.y = 0f;
			if (!Mathf.Approximately(base.Info.Range, 0f))
			{
				Vector3 vector = evt.RemoveData.Origin - evt.EffectData.Origin;
				vector.y = 0f;
				effectEvent.Range = evt.EffectData.Range - vector.magnitude;
				if (effectEvent.Range < 0f)
				{
					ReflectiveProjectile.Log.ErrorFormat("NEGATIVE RANGE:{0}, oldEffectRange:{1} - magnitude:{2}  |  removeDataOrigin:{3}  effectDataOrigin:{4}", new object[]
					{
						effectEvent.Range,
						evt.EffectData.Range,
						(evt.RemoveData.Origin - evt.EffectData.Origin).magnitude,
						evt.RemoveData.Origin,
						evt.EffectData.Origin
					});
					return -1;
				}
				effectEvent.Range = Mathf.Max(effectEvent.Range, this._latestProjectileRadius);
				effectEvent.LifeTime = effectEvent.Range / effectEvent.MoveSpeed;
			}
			else
			{
				effectEvent.LifeTime = this.MyInfo.ReflectLifetime;
			}
			effectEvent.Origin = evt.RemoveData.Origin;
			effectEvent.Target = effectEvent.Origin + Vector3.Reflect(evt.EffectData.Direction, evt.RemoveData.SrvNormal) * effectEvent.Range;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target);
			effectEvent.Modifiers = ModifierData.CopyData(modifiers);
			effectEvent.PreviousEventId = evt.RemoveData.TargetEventId;
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		protected override int FireExtraGadgetOnDeath(DestroyEffectMessage destroyEvt)
		{
			return -1;
		}

		protected virtual void EndEffect(DestroyEffectMessage evt, float totalDistance)
		{
			if (this.FireExtraOnEffectDeath.BoolGet() && this._endEffectId == -1)
			{
				this._endEffectId = this.FireExtraGadget();
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ReflectiveProjectile));

		private float _totalDistance;

		private float _latestProjectileRadius;

		private int _maxReflections;

		private ModifierData[] _reflectModifiers;

		private int _endEffectId;
	}
}
