using System;
using HeavyMetalMachines.Event;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BoomerBoomerang : BasicCannon
	{
		public BoomerBoomerangInfo MyInfo
		{
			get
			{
				return base.Info as BoomerBoomerangInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this.IncreasePctByHit = new Upgradeable(this.MyInfo.IncreasePctByHitUpgrade, this.IncreasePctByHit, this.MyInfo.UpgradesValues);
			this.LargerProjectile = new Upgradeable(this.MyInfo.LargerProjectileUpgrade, 0f, this.MyInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this.IncreasePctByHit.SetLevel(upgradeName, level);
			this.LargerProjectile.SetLevel(upgradeName, level);
		}

		protected override bool CanDestroyMyEffect(ref EffectRemoveEvent destroy)
		{
			destroy.WillCreateNextEvent = true;
			return base.CanDestroyMyEffect(ref destroy);
		}

		protected override int FireGadget()
		{
			EffectEvent effectEvent = (this.LargerProjectile != 0f) ? base.GetEffectEvent(this.MyInfo.EffectLarger) : base.GetEffectEvent(this.MyInfo.Effect);
			effectEvent.MoveSpeed = this._moveSpeed.Get();
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = this.DummyPosition(effectEvent);
			effectEvent.Target = base.Target;
			effectEvent.TargetId = this.TargetId;
			effectEvent.LifeTime = ((base.LifeTime <= 0f) ? (effectEvent.Range / effectEvent.MoveSpeed) : base.LifeTime);
			effectEvent.Modifiers = ModifierData.CopyData(this._damage);
			effectEvent.ExtraModifiers = ModifierData.CopyData(this.ExtraModifier);
			effectEvent.CustomVar = (byte)(this.IncreasePctByHit.Get() * 255f);
			base.SetTargetAndDirection(effectEvent);
			int result = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			if (this.FireNormalAndExtraEffectsTogether.BoolGet())
			{
				this.FireExtraGadget();
			}
			return result;
		}

		protected override int FireExtraGadgetOnDeath(DestroyEffect destroyEvt)
		{
			EffectEvent effectEvent = (this.LargerProjectile != 0f) ? base.GetEffectEvent(this.MyInfo.ExtraEffectLarger) : base.GetEffectEvent(this.MyInfo.ExtraEffect);
			effectEvent.Origin = destroyEvt.RemoveData.Origin;
			effectEvent.TargetId = this.Combat.Id.ObjId;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, this.Combat.Transform.position);
			effectEvent.MoveSpeed = ((this.ExtraMoveSpeed.Get() == 0f) ? this._moveSpeed.Get() : this.ExtraMoveSpeed.Get());
			effectEvent.Modifiers = ModifierData.CopyData(this._damage);
			effectEvent.ExtraModifiers = ModifierData.CopyData(this.ExtraModifier);
			effectEvent.CustomVar = (byte)(this.IncreasePctByHit.Get() * 255f);
			effectEvent.PreviousEventId = destroyEvt.RemoveData.TargetEventId;
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		protected Upgradeable IncreasePctByHit;

		protected Upgradeable LargerProjectile;
	}
}
