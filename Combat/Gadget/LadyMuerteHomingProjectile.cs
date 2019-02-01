using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class LadyMuerteHomingProjectile : GadgetBehaviour
	{
		public LadyMuerteHomingProjectileInfo MyInfo
		{
			get
			{
				return base.Info as LadyMuerteHomingProjectileInfo;
			}
		}

		public override void SetInfo(GadgetInfo gInfo)
		{
			base.SetInfo(gInfo);
			this._upgProjectileModifiers = ModifierData.CreateData(this.MyInfo.ProjectileModifiers, this.MyInfo);
			this._upgProjectileMoveSpeed = new Upgradeable(this.MyInfo.ProjectileMoveSpeedUpgrade, this.MyInfo.ProjectileMoveSpeed, this.MyInfo.UpgradesValues);
			this._upgUseProjectileWithImprovedHoming = new Upgradeable(this.MyInfo.UseProjectileWithImprovedHomingUpgrade, this.MyInfo.UseProjectileWithImprovedHoming, this.MyInfo.UpgradesValues);
			this._upgChargeCount = new Upgradeable(this.MyInfo.ChargeCountUpgrade, (float)this.MyInfo.ChargeCount, this.MyInfo.UpgradesValues);
			this._upgChargeTime = new Upgradeable(this.MyInfo.ChargeTimeUpgrade, this.MyInfo.ChargeTime, this.MyInfo.UpgradesValues);
			this.ChargeCount = Mathf.Min(this.ChargeCount, base.MaxChargeCount);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._upgProjectileModifiers.SetLevel(upgradeName, level);
			this._upgProjectileMoveSpeed.SetLevel(upgradeName, level);
			this._upgUseProjectileWithImprovedHoming.SetLevel(upgradeName, level);
			this.ChargeCount = Mathf.Min(this.ChargeCount, base.MaxChargeCount);
		}

		public override void Activate()
		{
			base.Activate();
			this.ChargeTime = (long)(base.MaxChargeTime * 1000f) + (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
		}

		protected override void Awake()
		{
			base.Awake();
			this.GadgetChargedUpdater = new GadgetWithChargesUpdater(GameHubBehaviour.Hub, this, new Action(base.GadgetUpdate), new Func<int>(this.FireProjectile));
			if (GameHubBehaviour.Hub)
			{
				this.CurrentCooldownTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			}
		}

		protected override void GadgetUpdate()
		{
			this.GadgetChargedUpdater.RunGadgetUpdate();
		}

		protected virtual int FireProjectile()
		{
			EffectEvent effectEvent;
			if (this._upgUseProjectileWithImprovedHoming == 0f)
			{
				effectEvent = base.GetEffectEvent(this.MyInfo.ProjectileEffect);
			}
			else
			{
				effectEvent = base.GetEffectEvent(this.MyInfo.ProjectileWithImprovedHomingEffect);
			}
			effectEvent.MoveSpeed = this._upgProjectileMoveSpeed;
			effectEvent.Range = this.MyInfo.ProjectileRange;
			effectEvent.Origin = this.Combat.Transform.position;
			effectEvent.Target = base.Target;
			effectEvent.LifeTime = effectEvent.Range / effectEvent.MoveSpeed;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target);
			effectEvent.Modifiers = this._upgProjectileModifiers;
			int num = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			base.OnGadgetUsed(num);
			return num;
		}

		public override float GetDps()
		{
			return base.GetDpsFromModifierData(this._upgProjectileModifiers);
		}

		protected GadgetWithChargesUpdater GadgetChargedUpdater;

		protected ModifierData[] _upgProjectileModifiers;

		protected Upgradeable _upgProjectileMoveSpeed;

		protected Upgradeable _upgUseProjectileWithImprovedHoming;

		protected Upgradeable _upgChargeCount;

		protected Upgradeable _upgChargeTime;
	}
}
