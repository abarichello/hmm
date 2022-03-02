using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BasicAttackAuto : BasicAttack
	{
		public new BasicAttackAutoInfo MyInfo
		{
			get
			{
				return base.Info as BasicAttackAutoInfo;
			}
		}

		protected override void GadgetUpdate()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.CurrentTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			base.BaseFixedUpdate();
			if (!this.Running)
			{
				return;
			}
			if (this.CurrentCooldownTime > this.CurrentTime)
			{
				return;
			}
			if (base.Pressed)
			{
				base.Pressed = false;
				this._shooting = !this._shooting;
				if (!this._shooting)
				{
					this._searchTargetCombat = null;
					this.CurrentCooldownTime = this.CurrentTime;
					return;
				}
				this.SearchTarget();
			}
			else
			{
				if (!this._shooting)
				{
					this.CurrentCooldownTime = this.CurrentTime;
					return;
				}
				if ((long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() - this._searchLastTimeMillis >= (long)this.MyInfo.SearchIntervalMillis)
				{
					this.SearchTarget();
				}
			}
			if (!this._searchTargetCombat)
			{
				this.CurrentCooldownTime = this.CurrentTime;
				return;
			}
			long num = this.CurrentTime - this.CurrentCooldownTime;
			this.CurrentCooldownTime = (long)(this.Cooldown * 1000f) + this.CurrentTime - num;
			this.FireCannon();
		}

		protected override void FireCannon()
		{
			BasicAttackInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.Effect);
			effectEvent.MoveSpeed = this._moveSpeed.Get();
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = this.DummyPosition();
			effectEvent.Target = CombatUtils.GetTargetInterceptPosition(this.Combat.Transform.position, this._moveSpeed, this._searchTargetCombat);
			effectEvent.LifeTime = effectEvent.Range / effectEvent.MoveSpeed;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target);
			base.UpdateDamage();
			effectEvent.Modifiers = this._damage;
			effectEvent = base.IncreaseRangeForPlayers(effectEvent);
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		private void SearchTarget()
		{
			this._searchTargetCombat = null;
			this._searchLastTimeMillis = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			Collider[] array = Physics.OverlapSphere(this.Combat.Transform.position, this.GetRange(), 1077054464);
			for (int i = 0; i < array.Length; i++)
			{
				CombatObject combat = CombatRef.GetCombat(array[i]);
				if (combat)
				{
					if (combat.IsAlive())
					{
						if (!combat.IsWard || (!(combat.WardEffect == null) && !(combat.WardEffect.Attached == this.Combat)))
						{
							if (combat.Team != this.Combat.Team)
							{
								float sqrMagnitude = (combat.transform.position - this.Combat.Transform.position).sqrMagnitude;
								BasicAttackAuto.list.Add(new KeyValuePair<CombatObject, float>(combat, sqrMagnitude));
							}
						}
					}
				}
			}
			if (BasicAttackAuto.list.Count == 0)
			{
				return;
			}
			int index = 0;
			float value = BasicAttackAuto.list[index].Value;
			for (int j = 0; j < BasicAttackAuto.list.Count; j++)
			{
				if (BasicAttackAuto.list[j].Value < value)
				{
					index = j;
					value = BasicAttackAuto.list[j].Value;
				}
			}
			this._searchTargetCombat = BasicAttackAuto.list[index].Key;
			BasicAttackAuto.list.Clear();
		}

		private CombatObject _searchTargetCombat;

		private long _searchLastTimeMillis;

		private bool _shooting;

		private static readonly List<KeyValuePair<CombatObject, float>> list = new List<KeyValuePair<CombatObject, float>>();
	}
}
