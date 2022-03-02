using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BounceableProjectile : ReflectiveProjectile
	{
		private BounceableProjectileInfo MyInfo
		{
			get
			{
				return base.Info as BounceableProjectileInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._bounceDamage = ModifierData.CreateData(this.MyInfo.AdditionalBounceModifiers, this.MyInfo);
			this.AdditionalBounces = new Upgradeable(this.MyInfo.AdditionalBouncesUpgrade, (float)this.MyInfo.AdditionalBounces, this.MyInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._bounceDamage.SetLevel(upgradeName, level);
			this.AdditionalBounces.SetLevel(upgradeName, level);
		}

		protected override int FireGadget()
		{
			int num = base.FireGadget();
			BounceableProjectile.Bounce item = new BounceableProjectile.Bounce
			{
				EffectId = num,
				BounceCount = 0
			};
			this._activeEffects.Add(item);
			return num;
		}

		public override bool CanDestroyEffect(ref EffectRemoveEvent destroy)
		{
			base.CanDestroyEffect(ref destroy);
			if (destroy.WillCreateNextEvent)
			{
				return true;
			}
			int effectIndex = this.GetEffectIndex(destroy.TargetEventId);
			if (this._activeEffects.Count <= 0 || effectIndex == -1)
			{
				return true;
			}
			bool flag = this._activeEffects[effectIndex].BounceCount <= 0;
			bool flag2 = this._activeEffects[effectIndex].BounceCount > 0;
			return (flag && !destroy.SrvWasScenery) || flag2;
		}

		private int GetEffectIndex(int targetEventId)
		{
			for (int i = 0; i < this._activeEffects.Count; i++)
			{
				if (this._activeEffects[i].EffectId == targetEventId)
				{
					return i;
				}
			}
			return -1;
		}

		protected override void InnerOnDestroyEffect(DestroyEffectMessage evt)
		{
			base.InnerOnDestroyEffect(evt);
			int effectIndex = this.GetEffectIndex(evt.RemoveData.TargetEventId);
			if (effectIndex == -1 || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this._activeEffects.RemoveAt(effectIndex);
		}

		protected override int ReflectEffect(DestroyEffectMessage evt, FXInfo effect, ModifierData[] modifiers)
		{
			int effectIndex = this.GetEffectIndex(evt.RemoveData.TargetEventId);
			if (this._activeEffects[effectIndex].BounceCount > 0)
			{
				BounceableProjectile.Log.DebugFormat("Reflecting Bounceable effect. Will use {0} effect and not {1}", new object[]
				{
					this.MyInfo.BounceEffect,
					effect
				});
				effect = this.MyInfo.BounceEffect;
			}
			int num = base.ReflectEffect(evt, effect, evt.EffectData.Modifiers);
			if (num != -1 && effectIndex != -1)
			{
				this._activeEffects[effectIndex].EffectId = num;
			}
			return num;
		}

		protected override void EndEffect(DestroyEffectMessage evt, float totalDistance)
		{
			if (this.FireExtraOnEffectDeath.BoolGet())
			{
				this.FireExtraGadget(evt.RemoveData.Origin);
			}
			int effectIndex = this.GetEffectIndex(evt.RemoveData.TargetEventId);
			if (this.AdditionalBounces.Get() > 0f && (float)this._activeEffects[effectIndex].BounceCount < this.AdditionalBounces.Get())
			{
				Vector3 normalized = (evt.RemoveData.Origin - evt.EffectData.Origin).normalized;
				Vector3 origin = evt.RemoveData.Origin;
				Vector3 to = evt.RemoveData.Origin + normalized * this.MyInfo.BounceDistance;
				this._activeEffects[effectIndex].BounceCount++;
				this._activeEffects[effectIndex].EffectId = base.FireGadgetFromTo(origin, to, this._bounceDamage, this.MyInfo.BounceEffect);
				base.ExistingFiredEffectsAdd(this._activeEffects[effectIndex].EffectId);
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BounceableProjectile));

		[SerializeField]
		private readonly List<BounceableProjectile.Bounce> _activeEffects = new List<BounceableProjectile.Bounce>();

		protected Upgradeable AdditionalBounces;

		protected ModifierData[] _bounceDamage;

		[Serializable]
		private class Bounce
		{
			public int EffectId;

			public int BounceCount;
		}
	}
}
