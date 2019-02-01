using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class CaterpillarBackThrow : GadgetBehaviour, DamageAreaCallback.IDamageAreaCallbackListener
	{
		public CaterpillarBackThrowInfo MyInfo
		{
			get
			{
				return base.Info as CaterpillarBackThrowInfo;
			}
		}

		public override void SetInfo(GadgetInfo gInfo)
		{
			base.SetInfo(gInfo);
			this._upgThrowLandingModifiers = ModifierData.CreateData(this.MyInfo.ThrowLandingModifiers, this.MyInfo);
			this._upgMaxUnits = new Upgradeable(this.MyInfo.MaxUnitsUpgrade, this.MyInfo.MaxUnits, this.MyInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._upgThrowLandingModifiers.SetLevel(upgradeName, level);
			this._upgMaxUnits.SetLevel(upgradeName, level);
		}

		protected override void Awake()
		{
			base.Awake();
			this.CurrentCooldownTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
		}

		private void Start()
		{
			this._myTransform = this.Combat.transform;
		}

		protected override void GadgetUpdate()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.CurrentTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			base.GadgetUpdate();
			if (this.CurrentCooldownTime > this.CurrentTime)
			{
				return;
			}
			if (!base.Pressed)
			{
				this.CurrentCooldownTime = this.CurrentTime;
				return;
			}
			if (!this.Combat.Controller.ConsumeEP((float)base.ActivationCost))
			{
				return;
			}
			long num = this.CurrentTime - this.CurrentCooldownTime;
			this.CurrentCooldownTime = (long)(this.Cooldown * 1000f) + this.CurrentTime - num;
			this.FireGadget();
		}

		protected override int FireGadget()
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.WarmupEffect);
			effectEvent.LifeTime = this.MyInfo.WarmupSeconds;
			effectEvent.Origin = this._myTransform.position + this._myTransform.forward * this.MyInfo.WarmupOffset;
			effectEvent.Target = effectEvent.Origin;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Origin + this._myTransform.forward);
			effectEvent.Range = this.MyInfo.WarmupRadius;
			this._currentWarmupEffect = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			base.OnGadgetUsed(this._currentWarmupEffect);
			return this._currentWarmupEffect;
		}

		private void ThrowCombatObject(CombatObject targetCombatObject)
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.ThrowEffect);
			Transform transform = targetCombatObject.transform;
			effectEvent.Origin = transform.position;
			effectEvent.Modifiers = this._upgThrowLandingModifiers;
			effectEvent.TargetId = targetCombatObject.Id.ObjId;
			effectEvent.LifeTime = this.MyInfo.ThrowLifeTime;
			Vector3 target = Vector3.zero;
			CaterpillarBackThrowInfo.DirectionEnum throwDirection = this.MyInfo.ThrowDirection;
			if (throwDirection != CaterpillarBackThrowInfo.DirectionEnum.Target)
			{
				if (throwDirection != CaterpillarBackThrowInfo.DirectionEnum.Backwards)
				{
					if (throwDirection == CaterpillarBackThrowInfo.DirectionEnum.Up)
					{
						target = effectEvent.Origin + this._myTransform.forward;
					}
				}
				else
				{
					target = effectEvent.Origin + this._myTransform.forward * -1f * this.MyInfo.ThrowDistance;
				}
			}
			else if (Vector3.Scale(base.Target - effectEvent.Origin, new Vector3(1f, 0f, 1f)).sqrMagnitude > this.MyInfo.ThrowDistance * this.MyInfo.ThrowDistance)
			{
				target = effectEvent.Origin + base.CalcDirection(effectEvent.Origin, base.Target) * this.MyInfo.ThrowDistance;
			}
			else
			{
				target = base.Target;
			}
			target.y = 0f;
			effectEvent.Target = target;
			effectEvent.Target = base.GetValidPosition(effectEvent.Origin, effectEvent.Target);
			effectEvent.Range = Vector3.Distance(effectEvent.Origin, effectEvent.Target);
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		public void OnDamageAreaCallback(DamageAreaCallback evt)
		{
			if (evt.Effect.EventId == this._currentWarmupEffect)
			{
				evt.DamagedPlayers.Sort(new Comparison<CombatObject>(this.Closer));
				int num = Mathf.Min(evt.DamagedPlayers.Count, (int)this._upgMaxUnits);
				for (int i = 0; i < num; i++)
				{
					CombatObject targetCombatObject = evt.DamagedPlayers[i];
					this.ThrowCombatObject(targetCombatObject);
				}
				this._currentWarmupEffect = -1;
			}
		}

		private int Closer(CombatObject x, CombatObject y)
		{
			float sqrMagnitude = (x.transform.position - this._myTransform.position).sqrMagnitude;
			float sqrMagnitude2 = (y.transform.position - this._myTransform.position).sqrMagnitude;
			return sqrMagnitude.CompareTo(sqrMagnitude2);
		}

		public override float GetRange()
		{
			return (!(this.MyInfo != null)) ? base.GetRange() : this.MyInfo.WarmupRadius;
		}

		public override float GetRangeSqr()
		{
			return (!(this.MyInfo != null)) ? base.GetRange() : (this.MyInfo.WarmupRadius * this.MyInfo.WarmupRadius);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CaterpillarBackThrow));

		private Transform _myTransform;

		private int _currentWarmupEffect = -1;

		private ModifierData[] _upgThrowLandingModifiers;

		private Upgradeable _upgMaxUnits;
	}
}
