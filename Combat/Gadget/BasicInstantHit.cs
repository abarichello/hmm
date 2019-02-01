using System;
using System.Collections;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Obsolete]
	public class BasicInstantHit : GadgetBehaviour
	{
		public BasicInstantHitInfo MyInfo
		{
			get
			{
				return base.Info as BasicInstantHitInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			BasicInstantHitInfo myInfo = this.MyInfo;
			this._damage = ModifierData.CreateData(myInfo.Damage, myInfo);
			this._selfDamage = ModifierData.CreateData(myInfo.SelfDamage, myInfo);
			this._moveSpeed = new Upgradeable(myInfo.MoveSpeedUpgrade, myInfo.MoveSpeed, myInfo.UpgradesValues);
			base.Pressed = false;
			this.CurrentCooldownTime = 0L;
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._damage.SetLevel(upgradeName, level);
			this._selfDamage.SetLevel(upgradeName, level);
			this._moveSpeed.SetLevel(upgradeName, level);
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
			if (this.CurrentCooldownTime == 0L || !base.Pressed)
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
			base.OnGadgetUsed(this.FireCannon());
		}

		protected virtual int FireCannon()
		{
			CombatObject target;
			float num;
			if (this.FireSphereCastHit(base.Radius, this.GetRange(), this.MyInfo.HitMask, out target, out num))
			{
				float time = num / this._moveSpeed.Get();
				base.StartCoroutine(this.DealDamage(target, time));
				return -1;
			}
			return -1;
		}

		private IEnumerator DealDamage(CombatObject target, float time)
		{
			yield return new WaitForSeconds(time);
			ModifierData[] newDamage = this._damage;
			ModifierData[] newSelfDamage = this._selfDamage;
			if (this.MyInfo.CasterMaxHealthThreshold > 0f)
			{
				float baseAmount = 100f * Mathf.Min(((float)this.Combat.Data.HPMax - this.Combat.Data.HP) / (float)this.Combat.Data.HPMax, this.MyInfo.CasterMaxHealthThreshold);
				newDamage = ModifierData.CreateConvoluted(newDamage, baseAmount);
				newSelfDamage = ModifierData.CreateConvoluted(newSelfDamage, baseAmount);
			}
			target.Controller.AddModifiers(newDamage, this.Combat, -1, false);
			this.Combat.Controller.AddModifiers(newSelfDamage, this.Combat, -1, false);
			yield break;
		}

		public override float GetDps()
		{
			ModifierData[] array = this._damage;
			if (this.MyInfo.CasterMaxHealthThreshold > 0f)
			{
				float baseAmount = 100f * Mathf.Min(((float)this.Combat.Data.HPMax - this.Combat.Data.HP) / (float)this.Combat.Data.HPMax, this.MyInfo.CasterMaxHealthThreshold);
				array = ModifierData.CreateConvoluted(array, baseAmount);
			}
			return base.GetDpsFromModifierData(array);
		}

		protected ModifierData[] _damage;

		protected ModifierData[] _selfDamage;

		protected Upgradeable _moveSpeed;

		private long _lastShot;
	}
}
