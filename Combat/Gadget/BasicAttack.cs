using System;
using System.Diagnostics;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BasicAttack : GadgetBehaviour
	{
		public BasicAttackInfo MyInfo
		{
			get
			{
				return base.Info as BasicAttackInfo;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BasicAttack.BasicAttackFiredListener ListenToBasicAttackFired;

		public override void SetInfo(GadgetInfo gInfo)
		{
			base.SetInfo(gInfo);
			BasicAttackInfo myInfo = this.MyInfo;
			this._damage = ModifierData.CreateData(myInfo.Modifiers, myInfo);
			this._moveSpeed = new Upgradeable(myInfo.MoveSpeedUpgrade, myInfo.MoveSpeed, myInfo.UpgradesValues);
			this._rigidbody = this.Combat.GetComponent<Rigidbody>();
			base.Pressed = false;
			this.ResetCooldown();
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._damage.SetLevel(upgradeName, level);
			this._moveSpeed.SetLevel(upgradeName, level);
		}

		public override float Cooldown
		{
			get
			{
				float cooldown = base.Cooldown;
				CombatAttributes attributes = this.Combat.Attributes;
				float fireRate = attributes.FireRate;
				float fireRatePct = attributes.FireRatePct;
				if (fireRate == 0f && fireRatePct == 0f)
				{
					return cooldown;
				}
				float num = 1f / cooldown;
				num *= 1f + fireRatePct;
				num += fireRate;
				return 1f / num;
			}
		}

		public void ResetCooldown()
		{
			this.CurrentCooldownTime = 0L;
		}

		protected override void GadgetUpdate()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.CurrentTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			base.GadgetUpdate();
			if (!this.Running)
			{
				return;
			}
			if (this.CurrentCooldownTime > this.CurrentTime)
			{
				return;
			}
			if (this.CurrentCooldownTime == 0L || !base.Pressed)
			{
				this.CurrentCooldownTime = this.CurrentTime;
				return;
			}
			long num = this.CurrentTime - this.CurrentCooldownTime;
			this.CurrentCooldownTime = (long)(this.Cooldown * 1000f) + this.CurrentTime - num;
			if (this.ListenToBasicAttackFired != null)
			{
				bool flag = false;
				this.ListenToBasicAttackFired(ref flag);
				if (flag)
				{
					return;
				}
			}
			this.FireCannon();
		}

		public float GetBasicDamage()
		{
			this.UpdateDamage();
			return this._damage[0].Amount;
		}

		public ModifierData[] GetAllDamage()
		{
			this.UpdateDamage();
			return this._damage;
		}

		protected void UpdateDamage()
		{
		}

		protected void BaseFixedUpdate()
		{
			base.GadgetUpdate();
		}

		protected virtual void FireCannon()
		{
			BasicAttackInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.Effect);
			effectEvent.MoveSpeed = this._moveSpeed.Get();
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = this.DummyPosition();
			effectEvent.Target = base.Target;
			effectEvent.LifeTime = effectEvent.Range / effectEvent.MoveSpeed;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target);
			this.UpdateDamage();
			effectEvent.Modifiers = this._damage;
			effectEvent = this.IncreaseRangeForPlayers(effectEvent);
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		public EffectEvent IncreaseRangeForPlayers(EffectEvent data)
		{
			if (!this.Combat.IsPlayer)
			{
				return data;
			}
			if (this.Combat.CustomGadget0 != this)
			{
				return data;
			}
			float num = data.Range / data.MoveSpeed;
			Vector3 vector = data.Target - data.Origin;
			Vector3 velocity = this._rigidbody.velocity;
			vector.y = 0f;
			velocity.y = 0f;
			Vector3 from = Vector3.Project(velocity, vector);
			data.Range += from.magnitude * num * (float)((Vector3.Angle(from, vector) >= 1f) ? -1 : 1);
			return data;
		}

		protected ModifierData[] _damage;

		protected Upgradeable _moveSpeed;

		private Rigidbody _rigidbody;

		public bool Running = true;

		public delegate void BasicAttackFiredListener(ref bool cancelBasicAttack);
	}
}
