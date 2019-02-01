using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class CaterpillarDelayedLineDamage : GadgetBehaviour, TickCallback.ITickCallbackCallbackListener, DamageAreaCallback.IDamageAreaCallbackListener
	{
		public CaterpillarDelayedLineDamageInfo MyInfo
		{
			get
			{
				return base.Info as CaterpillarDelayedLineDamageInfo;
			}
		}

		public override void SetInfo(GadgetInfo gInfo)
		{
			base.SetInfo(gInfo);
			this._upgExplosionModifiers = ModifierData.CreateData(this.MyInfo.ExplosionModifiers, this.MyInfo);
			this._upgKnockbackDistance = new Upgradeable(this.MyInfo.KnockbackDistanceUpgrade, this.MyInfo.KnockbackDistance, this.MyInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._upgExplosionModifiers.SetLevel(upgradeName, level);
			this._upgKnockbackDistance.SetLevel(upgradeName, level);
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
			if (!base.Pressed)
			{
				this._buttonHasBeenReleased = true;
			}
			if (this._buttonHasBeenReleased && base.Pressed && this._currentWarmupEffect != -1 && this._warmupReady)
			{
				this.DestroyWarmupEffect();
				return;
			}
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
			this._buttonHasBeenReleased = false;
			this.FireGadget();
		}

		private void DestroyWarmupEffect()
		{
			EffectRemoveEvent content = new EffectRemoveEvent
			{
				TargetEventId = this._currentWarmupEffect,
				TargetId = -1
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
		}

		protected override int FireGadget()
		{
			this._warmupDirection = base.CalcDirection(this._myTransform.position, this._myTransform.position + this._myTransform.forward * -1f);
			this._warmupPosition = this._myTransform.position + this._warmupDirection * this.MyInfo.ExplosionLength / 2f;
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.WarmupEffect);
			effectEvent.LifeTime = this.MyInfo.WarmupSeconds;
			effectEvent.Direction = this._warmupDirection;
			effectEvent.Origin = this._warmupPosition;
			this._currentWarmupEffect = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			return this._currentWarmupEffect;
		}

		private void FireExplosion()
		{
			this._currentWarmupEffect = -1;
			this._warmupReady = false;
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.ExplosionEffect);
			effectEvent.Direction = this._warmupDirection;
			effectEvent.Origin = this._warmupPosition;
			effectEvent.Modifiers = this._upgExplosionModifiers;
			this._currentExplosionEffect = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		protected override void InnerOnDestroyEffect(DestroyEffect evt)
		{
			if (evt.RemoveData.TargetEventId == this._currentWarmupEffect)
			{
				this.FireExplosion();
			}
		}

		public void OnTickCallback(TickCallback evt)
		{
			if (evt.Effect.EventId == this._currentWarmupEffect && !this._warmupReady)
			{
				this._warmupReady = true;
			}
		}

		public void OnDamageAreaCallback(DamageAreaCallback evt)
		{
			if (evt.Effect.EventId != this._currentExplosionEffect)
			{
				return;
			}
			if (this._upgKnockbackDistance.Get() <= 0f)
			{
				return;
			}
			this.FireKnockback(evt);
		}

		private void FireKnockback(DamageAreaCallback evt)
		{
			List<CombatObject> damagedPlayers = evt.DamagedPlayers;
			for (int i = 0; i < damagedPlayers.Count; i++)
			{
				CombatObject combatObject = damagedPlayers[i];
				if (!(combatObject == null))
				{
					if (combatObject.IsAlive())
					{
						EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.KnockbackEffect);
						if (BaseFX.CheckHit(this.Combat, combatObject, effectEvent))
						{
							Vector3 position = combatObject.transform.position;
							Vector3 vector = Vector3.Cross(this._warmupDirection, Vector3.up);
							Vector3 vector2 = vector * -1f;
							Plane plane = new Plane(vector, this._warmupPosition);
							Vector3 vector3;
							if (plane.GetSide(position))
							{
								vector3 = vector;
							}
							else
							{
								vector3 = vector2;
							}
							Vector3 target = position + vector3 * this._upgKnockbackDistance;
							effectEvent.TargetId = combatObject.Id.ObjId;
							effectEvent.Range = this._upgKnockbackDistance;
							effectEvent.Origin = position;
							target.y = 0f;
							effectEvent.Target = target;
							effectEvent.Direction = vector3;
							effectEvent.LifeTime = this.MyInfo.KnockbackFlyingTime;
							GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
						}
					}
				}
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CaterpillarDelayedLineDamage));

		private Transform _myTransform;

		private int _currentWarmupEffect = -1;

		private int _currentExplosionEffect = -1;

		private bool _buttonHasBeenReleased;

		private bool _warmupReady;

		private Vector3 _warmupDirection;

		private Vector3 _warmupPosition;

		private ModifierData[] _upgExplosionModifiers;

		private Upgradeable _upgKnockbackDistance;
	}
}
