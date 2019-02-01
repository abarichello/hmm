using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Event;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class GenkiDama : BasicCannon
	{
		private GenkiDamaInfo MyInfo
		{
			get
			{
				return base.Info as GenkiDamaInfo;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action ListenToGadgetEffectDestroyed;

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this.TrailModifier = ModifierData.CreateData(this.MyInfo.TrailModifier, this.MyInfo);
			this.OnDestroyEffectModifier = ModifierData.CreateData(this.MyInfo.OnDestroyEffectModifier, this.MyInfo);
			this.ColliderRadius = new Upgradeable(this.MyInfo.ColliderRadiusUpgrade, (float)this.MyInfo.ColliderRadius, this.MyInfo.UpgradesValues);
			this.DestroyOnHitPlayer = new Upgradeable(this.MyInfo.DestroyOnHitPlayerUpgrade, this.MyInfo.DestroyOnHitPlayer, this.MyInfo.UpgradesValues);
			this.TrailDropper = new Trail(GameHubBehaviour.Hub, new Func<FXInfo, EffectEvent>(base.GetEffectEvent), this.MyInfo.TrailEffect, this.MyInfo.TrailMustFollowCar);
			this.TrailDropper.SetLevel(this.TrailModifier, this.GetRange(), this._moveSpeed.Get(), (float)this.MyInfo.TrailColliderRadius, this.MyInfo.TrailPiecesLifeTime, this.MyInfo.TrailPiecesDropIntervalMillis);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this.TrailModifier.SetLevel(upgradeName, level);
			this.OnDestroyEffectModifier.SetLevel(upgradeName, level);
			this.ColliderRadius.SetLevel(upgradeName, level);
			this.DestroyOnHitPlayer.SetLevel(upgradeName, level);
			this.TrailDropper.SetLevel(this.TrailModifier, this.GetRange(), this._moveSpeed.Get(), (float)this.MyInfo.TrailColliderRadius, this.MyInfo.TrailPiecesLifeTime, this.MyInfo.TrailPiecesDropIntervalMillis);
		}

		protected override void Awake()
		{
			base.Awake();
			this.ActiveEffectsIds = new List<int>(4);
			if (GameHubBehaviour.Hub)
			{
				this.CurrentCooldownTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			}
		}

		protected override void GadgetUpdate()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			base.GadgetUpdate();
			this.TrailDropper.RunGadgetFixedUpdate();
		}

		protected override int FireGadget()
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.Effect);
			effectEvent.MoveSpeed = this._moveSpeed.Get();
			effectEvent.Range = this.GetRange();
			effectEvent.CustomVar = (byte)this.ColliderRadius.Get();
			effectEvent.Origin = this.DummyPosition();
			effectEvent.Target = base.Target;
			effectEvent.LifeTime = base.LifeTime;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target);
			effectEvent.Modifiers = this._damage;
			effectEvent.ExtraModifiers = this.ExtraModifier;
			base.SetTargetAndDirection(effectEvent);
			int num = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			this.ActiveEffectsIds.Add(num);
			this.TrailDropper.FireCannon(effectEvent.Origin, effectEvent.Direction, num, this.Combat);
			return num;
		}

		public override bool CanDestroyEffect(ref EffectRemoveEvent destroy)
		{
			return !this.ActiveEffectsIds.Contains(destroy.TargetEventId) || string.IsNullOrEmpty(this.MyInfo.DestroyOnHitPlayerUpgrade) || this.DestroyOnHitPlayer.BoolGet() || destroy.SrvWasScenery || !destroy.SrvOtherCollider;
		}

		protected override void InnerOnDestroyEffect(DestroyEffect evt)
		{
			if (!this.ActiveEffectsIds.Contains(evt.RemoveData.TargetEventId))
			{
				return;
			}
			this.TrailDropper.OnDestroyEffect(evt);
			this.ActiveEffectsIds.Remove(evt.RemoveData.TargetEventId);
			if (this.ListenToGadgetEffectDestroyed != null)
			{
				this.ListenToGadgetEffectDestroyed();
			}
			this.FireOnDestroyEffect(evt);
		}

		protected virtual void FireOnDestroyEffect(DestroyEffect evt)
		{
			if (string.IsNullOrEmpty(this.MyInfo.OnDestroyEffect.Effect))
			{
				return;
			}
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.OnDestroyEffect);
			effectEvent.Modifiers = this.OnDestroyEffectModifier;
			effectEvent.Origin = evt.RemoveData.Origin;
			effectEvent.Target = evt.RemoveData.Origin;
			effectEvent.Range = (float)this.MyInfo.OnDestroyEffectColliderRadius;
			effectEvent.LifeTime = this.MyInfo.OnDestroyEffectLifeTime;
			base.SetTargetAndDirection(effectEvent);
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		protected List<int> ActiveEffectsIds;

		protected Upgradeable ColliderRadius;

		protected Upgradeable DestroyOnHitPlayer;

		protected ModifierData[] TrailModifier;

		protected ModifierData[] OnDestroyEffectModifier;

		protected Trail TrailDropper;
	}
}
