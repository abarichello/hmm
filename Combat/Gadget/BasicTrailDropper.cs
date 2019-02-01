using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BasicTrailDropper : BasicCannon
	{
		private BasicTrailDropperInfo MyInfo
		{
			get
			{
				return base.Info as BasicTrailDropperInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this.TrailModifier = ModifierData.CreateData(this.MyInfo.TrailModifier, this.MyInfo);
			this.TrailDropper = new Trail(GameHubBehaviour.Hub, new Func<FXInfo, EffectEvent>(base.GetEffectEvent), this.MyInfo.TrailEffect, this.MyInfo.TrailMustFollowCar);
			this.TrailDropper.SetLevel(this.TrailModifier, this.GetRange(), this._moveSpeed.Get(), (float)this.MyInfo.TrailColliderRadius, this.MyInfo.TrailPiecesLifeTime, this.MyInfo.TrailPiecesDropIntervalMillis);
			this.UseTrail = new Upgradeable(this.MyInfo.UseTrailUpgrade, this.MyInfo.UseTrail, base.CannonInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this.TrailModifier.SetLevel(upgradeName, level);
			this.TrailDropper.SetLevel(this.TrailModifier, this.GetRange(), this._moveSpeed.Get(), (float)this.MyInfo.TrailColliderRadius, this.MyInfo.TrailPiecesLifeTime, this.MyInfo.TrailPiecesDropIntervalMillis);
			this.UseTrail.SetLevel(upgradeName, level);
		}

		protected override void GadgetUpdate()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			base.GadgetUpdate();
			if (this.UseTrail.BoolGet() && this.TrailOn)
			{
				this.TrailDropper.RunGadgetFixedUpdate();
			}
		}

		protected override int FireGadget(FXInfo effect, ModifierData[] modifier, Vector3 origin)
		{
			EffectEvent effectEvent = base.GetEffectEvent(effect);
			effectEvent.MoveSpeed = this._moveSpeed.Get();
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = origin;
			effectEvent.Target = base.Target;
			effectEvent.TargetId = this.TargetId;
			effectEvent.LifeTime = ((base.LifeTime <= 0f) ? (effectEvent.Range / effectEvent.MoveSpeed) : base.LifeTime);
			effectEvent.Modifiers = modifier;
			effectEvent.ExtraModifiers = ModifierData.CopyData(this.ExtraModifier);
			if (this.MyInfo.UseLastWarmupPosition)
			{
				effectEvent.Direction = this.LastWarmupDirection;
			}
			else
			{
				base.SetTargetAndDirection(effectEvent);
			}
			int num = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			if (this.UseTrail.BoolGet() && this.StartTrailWithFireGadget)
			{
				origin = GadgetBehaviour.DummyPosition(this.Combat, this.MyInfo.TrailEffect);
				this.TrailDropper.FireCannon(origin, effectEvent.Direction, num, this.Combat);
			}
			return num;
		}

		protected override void InnerOnDestroyEffect(DestroyEffect evt)
		{
			base.InnerOnDestroyEffect(evt);
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.TrailDropper.OnDestroyEffect(evt);
		}

		protected ModifierData[] TrailModifier;

		protected Trail TrailDropper;

		protected Upgradeable UseTrail;

		protected bool TrailOn = true;

		protected bool StartTrailWithFireGadget = true;
	}
}
