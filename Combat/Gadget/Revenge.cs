using System;
using System.Runtime.CompilerServices;
using HeavyMetalMachines.Event;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class Revenge : BasicCannon
	{
		private RevengeInfo MyInfo
		{
			get
			{
				return base.Info as RevengeInfo;
			}
		}

		protected override void OnPosDamageTaken(CombatObject causer, CombatObject taker, ModifierData mod, float amount, int eventid)
		{
			if (this.CurrentCooldownTime > this.CurrentTime || amount == 0f || !mod.Info.Effect.IsHPDamage() || !causer || taker != this.Combat || !this.CanFire(causer))
			{
				return;
			}
			this._lastCauser = causer;
			this._lastAmount = amount;
			base.Pressed = true;
		}

		protected override int FireWarmup(Action<EffectEvent> customizeData)
		{
			return base.FireWarmup(delegate(EffectEvent data)
			{
				data.TargetId = this._lastCauser.Id.ObjId;
			});
		}

		protected override int FireGadget()
		{
			base.Pressed = false;
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.Effect);
			effectEvent.MoveSpeed = this._moveSpeed.Get();
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = GadgetBehaviour.DummyPosition(this.Combat, this.MyInfo.Effect);
			effectEvent.Target = this._lastCauser.transform.position;
			effectEvent.TargetId = this._lastCauser.Id.ObjId;
			effectEvent.LifeTime = ((base.LifeTime <= 0f) ? (effectEvent.Range / effectEvent.MoveSpeed) : base.LifeTime);
			ModifierData[] damage = this._damage;
			float lastAmount = this._lastAmount;
			if (Revenge.<>f__mg$cache0 == null)
			{
				Revenge.<>f__mg$cache0 = new Func<ModifierData, bool>(ModifierDataExt.CanReturnDamage);
			}
			ModifierData[] modifiers = ModifierData.CreateConvolutedFiltering(damage, lastAmount, Revenge.<>f__mg$cache0);
			effectEvent.Modifiers = modifiers;
			effectEvent.ExtraModifiers = ModifierData.CopyData(this.ExtraModifier);
			base.SetTargetAndDirection(effectEvent);
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		private bool CanFire(CombatObject causer)
		{
			return (this.Combat.transform.position - causer.transform.position).magnitude < base.Radius;
		}

		private CombatObject _lastCauser;

		private float _lastAmount;

		[CompilerGenerated]
		private static Func<ModifierData, bool> <>f__mg$cache0;
	}
}
